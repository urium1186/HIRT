using DirectXTexNet;
using HaloInfiniteResearchTools.Models;
using ImageMagick;
using LibHIRT.Data.Textures;
using LibHIRT.Files.FileTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HaloInfiniteResearchTools.Services
{

    public class TextureConversionService : ITextureConversionService
    {

        public Task<TextureModel> LoadTexture(PictureFile file, float previewQuality = 1f)
        {
            var pict = file.Deserialize();
            return LoadTexture(file.Name, pict, previewQuality);
        }

        public async Task<TextureModel> LoadTexture(string fileName, S3DPicture file, float previewQuality = 1f)
        {
            var ddsImage = CreateDDS(file);
            var metadata = ddsImage.GetMetadata();

            var name = Path.GetFileNameWithoutExtension(fileName);
            var model = TextureModel.Create(name, file, ddsImage, metadata);

            await CreateImagePreviews(ddsImage, model, previewQuality);

            return model;
        }

        public async Task<Stream> GetDDSStream(PictureFile file)
        {
            var pict = file.Deserialize();

            var outStream = new MemoryStream();
            using (var ddsImage = CreateDDS(pict))
            using (var ddsStream = ddsImage.SaveToDDSMemory(DDS_FLAGS.NONE))
            {
                ddsStream.CopyTo(outStream);
                outStream.Position = 0;
                return outStream;
            }
        }

        public async Task<Stream> GetJpgStream(PictureFile file, float quality = 1f)
        {
            var pict = file.Deserialize();

            var outStream = new MemoryStream();
            using (var ddsImage = CreateDDS(pict))
            using (var compatImage = PrepareNonDDSImage(ddsImage))
            using (var jpgStream = compatImage.SaveToJPGMemory(0, quality))
            {
                jpgStream.CopyTo(outStream);
                outStream.Position = 0;
                return outStream;
            }
        }

        public async Task<MagickImageCollection> CreateMagickImageCollection(ScratchImage ddsImage, IEnumerable<int> indices = null)
        {
            var convertWasRequired = PrepareNonDDSImage(ddsImage, out var decompressedImage);

            var imageCount = ddsImage.GetImageCount();
            if (indices is null)
                indices = Enumerable.Range(0, imageCount);

            var idxSet = indices.ToHashSet();
            var imageDict = new ConcurrentDictionary<int, MagickImage>();
            Parallel.For(0, imageCount, i =>
            {
                if (!idxSet.Contains(i))
                {
                    imageDict[i] = new MagickImage();
                    return;
                }

                using (var imageStream = decompressedImage.SaveToDDSMemory(i, DDS_FLAGS.NONE))
                {
                    var image = new MagickImage(imageStream);
                    imageDict[i] = image;
                }
            });

            if (convertWasRequired)
                decompressedImage?.Dispose();

            var collection = new MagickImageCollection();
            for (var i = 0; i < imageCount; i++)
                collection.Add(imageDict[i]);

            return collection;
        }

        public async Task InvertGreenChannel(IMagickImage<float> image)
        {
            var channelCount = image.ChannelCount;
            var height = image.Height;
            var width = image.Width;

            using var pixels = image.GetPixels();
            for (var h = 0; h < height; h++)
            {
                var row = pixels.GetArea(0, h, width, 1);
                for (var w = 0; w < row.Length; w += channelCount)
                {
                    var y = row[w + 1] / 65535.0f;
                    y = 1.0f - y;
                    row[w + 1] = y * 65535.0f;
                }
                pixels.SetArea(0, h, width, 1, row);
            }
        }

        public async Task RecalculateZChannel(IMagickImage<float> image)
        {
            //image.ColorSpace = ColorSpace.sRGB;
            var channelCount = image.ChannelCount;
            var height = image.Height;
            var width = image.Width;

            using var pixels = image.GetPixels();
            for (var h = 0; h < height; h++)
            {
                var row = pixels.GetArea(0, h, width, 1);
                for (var w = 0; w < row.Length; w += channelCount)
                {
                    var x = row[w + 0] / 65535.0f;
                    var y = row[w + 1] / 65535.0f;
                    var z = MathF.Sqrt(1 - (x * x) + (y * y));
                    z = MathF.Max(0, MathF.Min(1, z)) * 65535.0f;

                    row[w + 2] = z;
                }
                pixels.SetArea(0, h, width, 1, row);
            }
        }

        #region Private Methods

        private ScratchImage CreateDDS(S3DPicture pict)
        {

            var format = DXGI_FORMAT.YUY2;
            if (pict.Format != S3DPictureFormat.UNSET)
                format = GetDxgiFormat(pict.Format);
            else
                format = GetDxgiFormat(pict.SFormat);
            // Create a scratch image based on the face count
            ScratchImage img;
            switch (pict.Type)
            {
                case "2D texture":

                    img = TexHelper.Instance.Initialize2D(format, pict.Width, pict.Height, 1, pict.MipMapCount, CP_FLAGS.NONE);
                    break;
                case "3D texture":
                    img = TexHelper.Instance.Initialize3D(format, pict.Width, pict.Height, pict.Depth, pict.MipMapCount, CP_FLAGS.NONE);
                    break;
                case "cube map":
                    img = TexHelper.Instance.InitializeCube(format, pict.Width, pict.Height, pict.Faces, pict.MipMapCount, CP_FLAGS.NONE);
                    break;
                case "array":
                    img = TexHelper.Instance.InitializeCube(format, pict.Width, pict.Height, pict.Faces, pict.MipMapCount, CP_FLAGS.NONE);
                    break;

                default:
                    img = TexHelper.Instance.InitializeCube(format, pict.Width, pict.Height, pict.Faces, pict.MipMapCount, CP_FLAGS.NONE);
                    break;
            }
            /*
              if ( pict.Faces == 1 )
                img = TexHelper.Instance.Initialize2D( format, pict.Width, pict.Height, 1, pict.MipMapCount, CP_FLAGS.NONE );
              else
                img = TexHelper.Instance.InitializeCube( format, pict.Width, pict.Height, pict.Faces, pict.MipMapCount, CP_FLAGS.NONE );
            */
            // Get a pointer to the scratch image's raw pixel data
            var srcData = pict.Data;
            var pDestLen = img.GetPixelsSize();
            var pDest = img.GetPixels();
            if (pDestLen < srcData.Length)
            {
                byte[] bufer = new byte[pDestLen];
                _ = new MemoryStream(pict.Data).Read(bufer);
                srcData = bufer;
            }
            Debug.Assert(pDestLen >= srcData.Length, "Source data will not fit in the destination image.");

            // Copy the data into the image
            Marshal.Copy(srcData, 0, pDest, srcData.Length);

            return img;
        }

        private async Task CreateImagePreviews(ScratchImage ddsImage, TextureModel model, float quality = 1f)
        {
            var convertWasRequired = PrepareNonDDSImage(ddsImage, out var compatImage);

            foreach (var image in model.Faces)
            {
                foreach (var mip in image.MipMaps)
                {
                    var mem = new MemoryStream();
                    using (var jpgStream = compatImage.SaveToJPGMemory(mip.ImageIndex, quality))
                    {
                        await jpgStream.CopyToAsync(mem);
                        mem.Position = 0;

                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        bitmap.CacheOption = BitmapCacheOption.Default;
                        bitmap.StreamSource = mem;
                        bitmap.UriSource = null;
                        bitmap.EndInit();

                        bitmap.Freeze();
                        mip.Preview = bitmap;
                    }
                }
            }

            if (convertWasRequired)
                compatImage?.Dispose();
        }

        private bool PrepareNonDDSImage(ScratchImage ddsImage, out ScratchImage rgbImage)
        {
            rgbImage = ddsImage;
            var format = ddsImage.GetMetadata().Format;
            if (format == DXGI_FORMAT.R8G8B8A8_UNORM)
                return false;

            if (format.ToString().StartsWith("BC"))
                rgbImage = ddsImage.Decompress(DXGI_FORMAT.R8G8B8A8_UNORM);
            else
                rgbImage = ddsImage.Convert(DXGI_FORMAT.R8G8B8A8_UNORM, TEX_FILTER_FLAGS.DEFAULT, 0);

            return true;
        }

        [Obsolete("You should only use this if you are planning on tossing the result.")]
        private ScratchImage PrepareNonDDSImage(ScratchImage ddsImage)
        {
            var format = ddsImage.GetMetadata().Format;
            if (format == DXGI_FORMAT.R8G8B8A8_UNORM)
                return ddsImage;

            if (format.ToString().StartsWith("BC"))
                return ddsImage.Decompress(DXGI_FORMAT.R8G8B8A8_UNORM);
            else
                return ddsImage.Convert(DXGI_FORMAT.R8G8B8A8_UNORM, TEX_FILTER_FLAGS.DEFAULT, 0);
        }

        private DXGI_FORMAT GetDxgiFormat(S3DPictureFormat format)
        {
            switch (format)
            {
                case S3DPictureFormat.A8R8G8B8:
                    return DXGI_FORMAT.R8G8B8A8_UNORM;
                case S3DPictureFormat.A8L8:
                    return DXGI_FORMAT.R8G8_UNORM;
                case S3DPictureFormat.OXT1:
                case S3DPictureFormat.AXT1:
                    return DXGI_FORMAT.BC1_UNORM;
                case S3DPictureFormat.DXT3:
                    return DXGI_FORMAT.BC2_UNORM;
                case S3DPictureFormat.DXT5:
                    return DXGI_FORMAT.BC3_UNORM;
                case S3DPictureFormat.X8R8G8B8:
                    return DXGI_FORMAT.B8G8R8X8_UNORM;
                case S3DPictureFormat.DXN:
                    return DXGI_FORMAT.BC5_UNORM;
                case S3DPictureFormat.DXT5A:
                    return DXGI_FORMAT.BC4_UNORM;
                case S3DPictureFormat.A16B16G16R16_F:
                    return DXGI_FORMAT.R16G16B16A16_FLOAT;
                case S3DPictureFormat.R9G9B9E5_SHAREDEXP:
                    return DXGI_FORMAT.R9G9B9E5_SHAREDEXP;
                default:
                    throw new NotImplementedException();
            }
        }
        private DXGI_FORMAT GetDxgiFormat(string format)
        {
            switch (format)
            {
                case "a8_unorm (000A)":
                    return DXGI_FORMAT.A8_UNORM;
                case "r8_unorm_rrr1 (RRR1)":
                    return DXGI_FORMAT.R8_UNORM;  // revisar
                case "r8_unorm_rrrr (RRRR)":
                    return DXGI_FORMAT.R8_UNORM;  // revisar
                case "r8g8_unorm_rrrg (RRRG)":
                    return DXGI_FORMAT.R8G8_UNORM;
                case "unused1":
                    return DXGI_FORMAT.UNKNOWN;
                case "unused2":
                    return DXGI_FORMAT.UNKNOWN;
                case "b5g6r5_unorm":
                    return DXGI_FORMAT.B5G6R5_UNORM;
                case "unused3":
                    return DXGI_FORMAT.UNKNOWN;
                case "b5g6r5a1_unorm":
                    return DXGI_FORMAT.B5G6R5_UNORM;
                case "b4g4r4a4_unorm":
                    return DXGI_FORMAT.B4G4R4A4_UNORM;
                case "b8g8r8x8_unorm":
                    return DXGI_FORMAT.B8G8R8X8_UNORM;
                case "b8g8r8a8_unorm":
                    return DXGI_FORMAT.B8G8R8A8_UNORM;
                case "unused4":
                    return DXGI_FORMAT.UNKNOWN;
                case "DEPRECATED_dxt5_bias_alpha":
                    return DXGI_FORMAT.UNKNOWN;
                case "bc1_unorm (dxt1)":
                    return DXGI_FORMAT.BC1_UNORM;
                case "bc2_unorm (dxt3)":
                    return DXGI_FORMAT.BC2_UNORM;
                case "bc3_unorm (dxt5)":
                    return DXGI_FORMAT.BC3_UNORM;
                case "DEPRECATED_a4r4g4b4_font":
                    return DXGI_FORMAT.UNKNOWN;
                case "unused7":
                    return DXGI_FORMAT.UNKNOWN;
                case "unused8":
                    return DXGI_FORMAT.UNKNOWN;
                case "DEPRECATED_SOFTWARE_rgbfp32":
                    return DXGI_FORMAT.UNKNOWN;
                case "unused9":
                    return DXGI_FORMAT.UNKNOWN;
                case "r8g8_snorm (v8u8)":
                    return DXGI_FORMAT.R8G8_SNORM;
                case "DEPRECATED_g8b8":
                    return DXGI_FORMAT.UNKNOWN;
                case "r32g32b32a32_float (abgrfp32)":
                    return DXGI_FORMAT.R32G32B32A32_FLOAT;
                case "r16g16b16a16_float (abgrfp16)":
                    return DXGI_FORMAT.R16G16B16A16_FLOAT;
                case "r16_float_rrr1 (16f_mono)":
                    return DXGI_FORMAT.R16_FLOAT;
                case "r16_float_r000 (16f_red)":
                    return DXGI_FORMAT.R16_FLOAT;
                case "r8g8b8a8_snorm (q8w8v8u8)":
                    return DXGI_FORMAT.R8G8B8A8_SNORM;
                case "r10g10b10a2_unorm (a2r10g10b10)":
                    return DXGI_FORMAT.R10G10B10A2_UNORM;
                case "r16g16b16a16_unorm (a16b16g16r16)":
                    return DXGI_FORMAT.R16G16B16A16_UNORM;
                case "r16g16_snorm (v16u16)":
                    return DXGI_FORMAT.R16G16_SNORM;
                case "r16_unorm_rrr0 (L16)":
                    return DXGI_FORMAT.R16_UNORM;
                case "r16g16_unorm (r16g16)":
                    return DXGI_FORMAT.R16G16_UNORM;
                case "r16g16b16a16_snorm (signedr16g16b16a16)":
                    return DXGI_FORMAT.R16G16B16A16_SNORM;
                case "DEPRECATED_dxt3a":
                    return DXGI_FORMAT.UNKNOWN;
                case "bc4_unorm_rrrr (dxt5a)":
                    return DXGI_FORMAT.BC4_UNORM;
                case "bc4_snorm_rrrr":
                    return DXGI_FORMAT.BC4_SNORM;
                case "DEPRECATED_dxt3a_1111":
                    return DXGI_FORMAT.UNKNOWN;
                case "bc5_snorm (dxn)":
                    return DXGI_FORMAT.BC5_SNORM;
                case "DEPRECATED_ctx1":
                    return DXGI_FORMAT.UNKNOWN;
                case "DEPRECATED_dxt3a_alpha_only":
                    return DXGI_FORMAT.UNKNOWN;
                case "DEPRECATED_dxt3a_monochrome_only":
                    return DXGI_FORMAT.UNKNOWN;
                case "bc4_unorm_000r (dxt5a_alpha)":
                    return DXGI_FORMAT.BC4_UNORM;
                case "bc4_unorm_rrr1 (dxt5a_mono)":
                    return DXGI_FORMAT.BC4_UNORM;
                case "bc5_unorm_rrrg (dxn_mono_alpha)":
                    return DXGI_FORMAT.BC5_UNORM;
                case "bc5_snorm_rrrg (dxn_mono_alpha signed)":
                    return DXGI_FORMAT.BC5_SNORM;
                case "bc6h_uf16 ":
                    return DXGI_FORMAT.BC6H_UF16;
                case "bc6h_sf16 ":
                    return DXGI_FORMAT.BC6H_SF16;
                case "bc7_unorm ":
                    return DXGI_FORMAT.BC7_UNORM;
                case "d24_unorm_s8_uint (depth 24)":
                    return DXGI_FORMAT.D24_UNORM_S8_UINT;
                case "r11g11b10_float":
                    return DXGI_FORMAT.R11G11B10_FLOAT;
                case "r16g16_float":
                    return DXGI_FORMAT.R16G16_FLOAT;
                default:
                    throw new NotImplementedException();
            }
        }


        #endregion

    }

}
