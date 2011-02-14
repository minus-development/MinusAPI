//   Copyright 2010 Bruno de Carvalho
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace BiasedBit.MinusEngine
{
    /// <summary>
    /// Result of a CreateGallery operation.
    /// </summary>
    public class CreateGalleryResult
    {
        #region Constructors
        public CreateGalleryResult()
        {
        }
        
        public CreateGalleryResult(String editorId, String readerId, String key)
        {
            this.EditorId = editorId;
            this.ReaderId = readerId;
            this.Key = key;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The editor (private) unique id assigned to the new gallery.
        /// Use this id when modifying the gallery.
        /// </summary>
        [JsonProperty("editor_id")]
        public String EditorId { get; set; }

        /// <summary>
        /// The reader (public) unique id assigned to the new gallery.
        /// </summary>
        [JsonProperty("reader_id")]
        public String ReaderId { get; set; }

        /// <summary>
        /// The key (password) to modify the gallery (via the editor id).
        /// </summary>
        [JsonProperty("key")]
        public String Key { get; set; }
        #endregion

        #region Low level overrides
        public override string ToString()
        {
            return new StringBuilder("CreateGalleryResult{EditorId=")
                .Append(this.EditorId)
                .Append(", ReaderId=")
                .Append(this.ReaderId)
                .Append(", Key=")
                .Append(this.Key)
                .Append('}').ToString();
        }
        #endregion
    }
}
