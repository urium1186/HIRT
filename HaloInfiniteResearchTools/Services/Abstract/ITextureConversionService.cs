﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DirectXTexNet;
using HaloInfiniteResearchTools.Models;
using ImageMagick;
using LibHIRT.Data.Textures;
using LibHIRT.Files.FileTypes;

namespace HaloInfiniteResearchTools.Services
{

  public interface ITextureConversionService
  {
        
    Task<TextureModel> LoadTexture( PictureFile file, float previewQuality = 1f );
    Task<TextureModel> LoadTexture( string name, S3DPicture file, float previewQuality = 1f );

    Task<Stream> GetDDSStream( PictureFile file );
    Task<Stream> GetJpgStream( PictureFile file, float quality = 1f );
    
    Task<MagickImageCollection> CreateMagickImageCollection( ScratchImage ddsImage, IEnumerable<int> indices = null );
    Task InvertGreenChannel( IMagickImage<float> image );
    Task RecalculateZChannel( IMagickImage<float> image );

  }

}
