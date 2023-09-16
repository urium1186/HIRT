using HaloInfiniteResearchTools.Common.Extensions;
using HaloInfiniteResearchTools.Common.Grunt.Converters;
using HaloInfiniteResearchTools.Common.Grunt.Util;
using OpenSpartan.Grunt.Core;
using OpenSpartan.Grunt.Models;
using OpenSpartan.Grunt.Models.HaloInfinite;
using OpenSpartan.Grunt.Util;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Common.Grunt
{
    public class HaloInfiniteClientFix : HaloInfiniteClient
    {

        private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                (JsonConverter)new EmptyDateStringToNullJsonConverter(),
                (JsonConverter)new OnlineUriReferenceConverter(),
                (JsonConverter)new AcknowledgementTypeConverter(),
                (JsonConverter)new XmlDurationToTimeSpanJsonConverter()
            }
        };

        public HaloInfiniteClientFix(string spartanToken, string xuid = "", string clearanceToken = "")
        {
            base.SpartanToken = spartanToken;
            base.Xuid = xuid;
            base.ClearanceToken = clearanceToken;
        }
        public HaloInfiniteClientFix() { }

        //
        // Summary:
        //     Gets a specific item from the Game CMS, such as armor emplems, weapon cores,
        //     vehicle cores, and others.
        //
        // Parameters:
        //   itemPath:
        //     Path to the item to be obtained. Example is "/inventory/armor/emblems/013-001-363f4a25.json".
        //
        //   flightId:
        //     Unique ID for the currently active flight.
        //
        // Returns:
        //     If successful, an instance of InGameItem. Otherwise, null.
        //
        // Remarks:
        //     For example, you may find that you can get the data about an armor emblem with
        //     the path "/inventory/armor/emblems/013-001-363f4a25.json".
        public async Task<HaloApiResultContainer<InGameItem, HaloApiErrorContainer>> GameCmsGetItemFix(string itemPath, string flightId)
        {
            HaloInfiniteClientFix haloInfiniteClient = this;
            StringBuilder defaultInterpolatedStringHandler = new StringBuilder();
            defaultInterpolatedStringHandler.Append("https://");
            defaultInterpolatedStringHandler.Append(HaloCoreEndpoints.GameCmsOrigin);
            defaultInterpolatedStringHandler.Append(".");
            defaultInterpolatedStringHandler.Append(HaloCoreEndpoints.ServiceDomain);
            defaultInterpolatedStringHandler.Append("/hi/Progression/file/");
            defaultInterpolatedStringHandler.Append(itemPath);
            defaultInterpolatedStringHandler.Append("?flight=");
            defaultInterpolatedStringHandler.Append(flightId);
            return await haloInfiniteClient.ExecuteAPIRequestFix<InGameItem>(defaultInterpolatedStringHandler.ToString(), HttpMethod.Get, useSpartanToken: true, useClearance: true, GlobalConstants.HALO_WAYPOINT_USER_AGENT);
        }
        public async Task<HaloApiResultContainer<ArmorTheme, HaloApiErrorContainer>> GameCmsGetArmorTheme(string itemPath, string flightId)
        {
            HaloInfiniteClientFix haloInfiniteClient = this;
            StringBuilder defaultInterpolatedStringHandler = new StringBuilder();
            defaultInterpolatedStringHandler.Append("https://");
            defaultInterpolatedStringHandler.Append(HaloCoreEndpoints.GameCmsOrigin);
            defaultInterpolatedStringHandler.Append(".");
            defaultInterpolatedStringHandler.Append(HaloCoreEndpoints.ServiceDomain);
            defaultInterpolatedStringHandler.Append("/hi/Progression/file/");
            defaultInterpolatedStringHandler.Append(itemPath);
            defaultInterpolatedStringHandler.Append("?flight=");
            defaultInterpolatedStringHandler.Append(flightId);
            return await haloInfiniteClient.ExecuteAPIRequestFix<ArmorTheme>(defaultInterpolatedStringHandler.ToString(), HttpMethod.Get, useSpartanToken: true, useClearance: true, GlobalConstants.HALO_WAYPOINT_USER_AGENT);
        }

        public async Task<HaloApiResultContainer<T, HaloApiErrorContainer>> ExecuteAPIRequestFix<T>(string endpoint, HttpMethod method, bool useSpartanToken, bool useClearance, string userAgent, string content = "", ApiContentType contentType = ApiContentType.Json)
        {
            string headerValue = contentType.GetHeaderValue();
            HttpClient httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli)
            });
            HaloApiResultContainer<T, HaloApiErrorContainer> resultContainer = new HaloApiResultContainer<T, HaloApiErrorContainer>(default(T), new HaloApiErrorContainer());
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(endpoint),
                Method = method
            };
            if (!string.IsNullOrEmpty(content))
            {
                httpRequestMessage.Content = new StringContent(content, Encoding.UTF8, headerValue);
            }

            if (httpRequestMessage.Method == HttpMethod.Post || httpRequestMessage.Method == HttpMethod.Put || httpRequestMessage.Method == HttpMethod.Patch)
            {
                HttpRequestMessage httpRequestMessage2 = httpRequestMessage;
                if (httpRequestMessage2.Content == null)
                {
                    httpRequestMessage2.Content = new StringContent(string.Empty);
                }

                httpRequestMessage.Content!.Headers.ContentType = new MediaTypeHeaderValue((headerValue != null) ? headerValue : "application/json");
            }

            if (useSpartanToken)
            {
                httpRequestMessage.Headers.Add("x-343-authorization-spartan", SpartanToken);
            }

            if (useClearance)
            {
                httpRequestMessage.Headers.Add("343-clearance", ClearanceToken);
            }

            httpRequestMessage.Headers.Add("User-Agent", userAgent);
            httpRequestMessage.Headers.Add("Accept", "application/json");
            httpRequestMessage.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
            resultContainer.Error!.Code = Convert.ToInt32(response.StatusCode);
            if (response.IsSuccessStatusCode)
            {
                if (typeof(T) == typeof(string))
                {
                    HaloApiResultContainer<T, HaloApiErrorContainer> haloApiResultContainer = resultContainer;
                    haloApiResultContainer.Result = (T)Convert.ChangeType(await response.Content.ReadAsStringAsync(), typeof(T));
                }
                else if (typeof(T) == typeof(byte[]))
                {
                    using MemoryStream memoryStream = new MemoryStream();
                    response.Content.ReadAsStreamAsync().Result.CopyTo(memoryStream);
                    resultContainer.Result = (T)Convert.ChangeType(memoryStream.ToArray(), typeof(T));
                }
                else if (typeof(T) == typeof(bool))
                {
                    resultContainer.Result = (T)(object)response.IsSuccessStatusCode;
                }
                else
                {
                    if (Attribute.GetCustomAttribute(typeof(T), typeof(IsAutomaticallySerializableAttribute)) == null && !typeof(T).IsGenericType)
                    {
                        throw new NotSupportedException("The specified type is not supported. You can only get results in string or byte array formats.");
                    }

                    string text = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        try
                        {
                            resultContainer.Result = JsonSerializer.Deserialize<T>(text, serializerOptions);
                        }
                        catch (Exception exs)
                        {


                        }
                    }

                }
            }

            if (response.Content != null)
            {
                HaloApiErrorContainer error = resultContainer.Error;
                error.Message = await response.Content.ReadAsStringAsync();
            }

            return resultContainer;
        }
    }
}
