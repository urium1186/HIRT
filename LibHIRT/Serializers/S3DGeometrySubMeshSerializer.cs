﻿using System.Collections.Generic;
using System.IO;
using System.Numerics;
using LibHIRT.Common;
using LibHIRT.Data;
using LibHIRT.Data.Materials;
using LibHIRT.Serializers.Configurations;
using static LibHIRT.Assertions;

namespace LibHIRT.Serializers
{

  public class S3DGeometrySubMeshSerializer : SerializerBase<List<S3DGeometrySubMesh>>
  {

    private S3DGeometryGraph GeometryGraph { get; }

    public S3DGeometrySubMeshSerializer( S3DGeometryGraph geometryGraph )
    {
      GeometryGraph = geometryGraph;
    }

    protected override void OnDeserialize( BinaryReader reader, List<S3DGeometrySubMesh> submeshes )
    {
      var count = GeometryGraph.SubMeshCount;

      for ( var i = 0; i < count; i++ )
        submeshes.Add( new S3DGeometrySubMesh() );
      /*
      while ( reader.BaseStream.Position < sectionEndOffset )
      {
        var sentinel = ( SubMeshSentinel ) reader.ReadUInt16();
        var endOffset = reader.ReadUInt32();

        switch ( sentinel )
        {
          case SubMeshSentinel.BufferInfo:
            ReadBufferInfo( reader, submeshes, endOffset );
            break;
          case SubMeshSentinel.MeshIds:
            ReadMeshIds( reader, submeshes );
            break;
          case SubMeshSentinel.Unk_02:
            ReadUnk02( reader, submeshes );
            break;
          case SubMeshSentinel.BoneMap:
            ReadBoneIds( reader, submeshes );
            break;
          case SubMeshSentinel.UvScaling:
            ReadUvScaling( reader, submeshes );
            break;
          case SubMeshSentinel.TransformInfo:
            ReadTransformInfo( reader, submeshes );
            break;
          case SubMeshSentinel.Materials_String:
            ReadMaterialsString( reader, submeshes );
            break;
          case SubMeshSentinel.Materials_Static:
            ReadMaterialsStatic( reader, submeshes );
            break;
          case SubMeshSentinel.Materials_Dynamic:
            ReadMaterialsDynamic( reader, submeshes );
            break;
          default:
            Fail( $"Unknown SubMesh Sentinel: {sentinel:X}" );
            break;
        }

        Assert( reader.BaseStream.Position == endOffset,
          "Reader position does not match the submesh sentinel's end offset." );
      }

      Assert( reader.BaseStream.Position == sectionEndOffset,
          "Reader position does not match the submesh section's end offset." );*/
    }

    private void ReadBufferInfo( BinaryReader reader, List<S3DGeometrySubMesh> submeshes, long endOffset )
    {
      // TODO: This is a bit hacky in order to determine whether or not we should do the seek nonsense.
      if ( submeshes.Count == 0 )
        return;

      const int BUFFER_INFO_SIZE = 0xC;
      var sectionSize = endOffset - reader.BaseStream.Position;
      var elementSize = sectionSize / submeshes.Count;
      bool hasAdditionalData = elementSize > BUFFER_INFO_SIZE;

      foreach ( var submesh in submeshes )
      {
        submesh.BufferInfo = new S3DSubMeshBufferInfo
        {
          VertexOffset = reader.ReadUInt16(),
          VertexCount = reader.ReadUInt16(),
          FaceOffset = reader.ReadUInt16(),
          FaceCount = reader.ReadUInt16(),
          NodeId = reader.ReadUInt16(),
          SkinCompoundId = reader.ReadInt16()
        };

        /* Not sure what this data is, but we're just gonna seek past it for now.
         * It has something to do with a flag set earlier in the file.
         * This implementation of looping 32-bits and doing a left rotate seems to be correct.
         * That's how it's done in the disassembly.
         */
        if ( hasAdditionalData )
        {
          var seekFlags = reader.ReadUInt32();

          for ( var i = 0; i < 32; i++ )
          {
            if ( ( seekFlags & 1 ) != 0 )
            {
              // These appear to be floats
              var a = reader.ReadSingle();
              var b = reader.ReadSingle();
            }
            seekFlags = ( seekFlags << 1 ) | ( seekFlags >> 31 );
          }
        }
      }
    }

    private void ReadMeshIds( BinaryReader reader, List<S3DGeometrySubMesh> submeshes )
    {
      foreach ( var submesh in submeshes )
        submesh.MeshId = reader.ReadUInt32();
    }

    private void ReadUnk02( BinaryReader reader, List<S3DGeometrySubMesh> submeshes )
    {
      // TODO
      /* Not sure what this is. Just like the BufferInfo section, 
       * this does the same weird reading pattern at the end.
       * Seems to be a nested section and some scripting.
       */
      // TODO: Move this to own serializer once we figure out that it is

      foreach ( var submesh in submeshes )
      {
        var unk_01 = reader.ReadUInt16();
        var count = reader.ReadByte();

        while ( true )
        {
          var sentinel = reader.ReadUInt16();
          var endOffset = reader.ReadUInt32();

          switch ( sentinel )
          {
            case 0x0000:
            {
              for ( var i = 0; i < count; i++ )
              {
                _ = reader.ReadInt16();
                _ = reader.ReadInt16();
                _ = reader.ReadByte();
                _ = reader.ReadByte();
              }
            }
            break;
            case 0x0001:
            {
              for ( var i = 0; i < count; i++ )
              {
                _ = reader.ReadByte();
                _ = reader.ReadByte();
              }
            }
            break;
            case 0x0002:
            {
              for ( var i = 0; i < count; i++ )
              {
                _ = reader.ReadByte();
                _ = reader.ReadByte();
              }
            }
            break;
            case 0x0003:
            {
              for ( var i = 0; i < count; i++ )
                _ = reader.ReadPascalString32();
            }
            break;
            case 0xFFFF:
              break;
            default:
              Fail( $"Unknown sentinel: {sentinel:X2}" );
              break;
          }

          Assert( reader.BaseStream.Position == endOffset );

          if ( sentinel == 0xFFFF )
            break;
        }

        var seekFlags = reader.ReadUInt32();
        for ( var i = 0; i < 32; i++ )
        {
          if ( ( seekFlags & 1 ) != 0 )
          {
            // These appear to be floats
            var a = reader.ReadSingle();
            var b = reader.ReadSingle();
          }
          seekFlags = ( seekFlags << 1 ) | ( seekFlags >> 31 );
        }

      }

    }

    private void ReadBoneIds( BinaryReader reader, List<S3DGeometrySubMesh> submeshes )
    {
      foreach ( var submesh in submeshes )
      {
        var mesh = GeometryGraph.Meshes[ ( int ) submesh.MeshId ];
        if ( !mesh.Flags.HasFlag( S3DGeometryMeshFlags.HasBoneIds ) )
          continue;

        var count = reader.ReadByte();
        var boneIds = submesh.BoneIds = new short[ count ];
        for ( var i = 0; i < count; i++ )
          boneIds[ i ] = reader.ReadInt16();
      }
    }

    private void ReadUvScaling( BinaryReader reader, List<S3DGeometrySubMesh> submeshes )
    {
      foreach ( var submesh in submeshes )
      {
        var mesh = GeometryGraph.Meshes[ ( int ) submesh.MeshId ];
        if ( !mesh.Flags.HasFlag( S3DGeometryMeshFlags.HasUvScaling ) )
          continue;

        var count = reader.ReadByte();
        var data = submesh.UvScaling = new Dictionary<byte, short>( count );
        for ( var i = 0; i < count; i++ )
        {
          var uvSetIndex = reader.ReadByte();
          var uvScale = reader.ReadInt16();
          data[ uvSetIndex ] = uvScale;
        }
      }
    }

    private void ReadTransformInfo( BinaryReader reader, List<S3DGeometrySubMesh> submeshes )
    {
      // TODO:
      /* This is probably transform data, but sometimes it behaves inconsistently.
       */

      foreach ( var submesh in submeshes )
      {
        var mesh = GeometryGraph.Meshes[ ( int ) submesh.MeshId ];
        if ( !mesh.Flags.HasFlag( S3DGeometryMeshFlags.HasTransformInfo ) )
          continue;

        submesh.Position = new Vector3(
          x: reader.ReadInt16(),
          y: reader.ReadInt16(),
          z: reader.ReadInt16()
        );

        // TODO: Are int16s correct?
        submesh.Scale = new Vector3(
          x: reader.ReadInt16(),
          y: reader.ReadInt16(),
          z: reader.ReadInt16()
        );
      }
    }

    /* The next three properties are all for materials. The material data itself is always structured
     * the same (see the material serializers). However, for whatever reason, the data can be represented
     * in three different ways:
     * 
     *  - Materials (String):  All of the data is stored in a pascal string. It seems to be formatted similarly
     *                         to their scripting language, which is somewhat similar to JSON.
     *                        
     *  - Materials (Static):  Each property starts with a name (null-terminated string), followed by an unknown
     *                         32-bit int, followed by the data type (see the enum in the serializer), followed
     *                         by the property's _intValue.
     *                        
     *  - Materials (Dynamic): The most commonly used form.
     *                         Each property starts with a name (pascal string) followed by the data type
     *                         (see the enum in the serializer), followed by the property's _intValue.
     *                         
     *  They just couldn't make it simple, could they?
     */

    private void ReadMaterialsString( BinaryReader reader, List<S3DGeometrySubMesh> submeshes )
    {
      /* Materials with this sentinel are represented as just a string.
       */
      var serializer = new StringScriptingSerializer<S3DMaterial>();
      foreach ( var submesh in submeshes )
      {
        submesh.NodeId = reader.ReadUInt16();
        submesh.Material = serializer.Deserialize( reader );
      }
    }

    private void ReadMaterialsStatic( BinaryReader reader, List<S3DGeometrySubMesh> submeshes )
    {
      /* Materials with this sentinel are similar to the MaterialsDynamic section, but the strings 
       * are null-terminated and there's an additional int32 after the property name.
       */
      var serializer = new StaticBinaryScriptingSerializer<S3DMaterial>();
      foreach ( var submesh in submeshes )
      {
        submesh.NodeId = reader.ReadUInt16();
        submesh.Material = serializer.Deserialize( reader );
      }
    }

    private void ReadMaterialsDynamic( BinaryReader reader, List<S3DGeometrySubMesh> submeshes )
    {
      /* This is the most common form of materials.
       * The properties are pascal strings, followed by the data type, and then the _intValue.
       */
      var serializer = new DynamicBinaryScriptingSerializer<S3DMaterial>();
      foreach ( var submesh in submeshes )
      {
        submesh.NodeId = reader.ReadUInt16();
        submesh.Material = serializer.Deserialize( reader );
      }
    }

    private enum SubMeshSentinel : ushort
    {
      BufferInfo = 0x0000,
      MeshIds = 0x0001,
      Unk_02 = 0x0002,
      BoneMap = 0x0003,
      UvScaling = 0x0004,
      TransformInfo = 0x0005,
      Materials_String = 0x0006,
      Materials_Static = 0x0007,
      Materials_Dynamic = 0x0008
    }

  }

}
