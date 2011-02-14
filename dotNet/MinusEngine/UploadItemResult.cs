using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BiasedBit.MinusEngine
{
    public class UploadItemResult
    {
        #region Constructors
        public UploadItemResult()
        {
        }

        public UploadItemResult(String id, int height, int width, String filesize)
        {
            this.Id = id;
            this.Height = height;
            this.Width = width;
            this.Filesize = filesize;
        }
        #endregion

        #region Fields
        [JsonProperty("id")]
        public String Id { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("filesize")]
        public String Filesize { get; set; }
        #endregion

        #region Low level overrides
        public override string ToString()
        {
            return new StringBuilder("UploadItemResult{")
                .Append("Id=").Append(this.Id)
                .Append(", Height=").Append(this.Height)
                .Append(", Width=").Append(this.Width)
                .Append(", Filesize=").Append(this.Filesize)
                .Append('}').ToString();
        }
        #endregion
    }
}
