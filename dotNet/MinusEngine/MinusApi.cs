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
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace BiasedBit.MinusEngine
{
    public delegate void CreateGalleryCompleteHandler(MinusApi sender, CreateGalleryResult result);
    public delegate void CreateGalleryFailedHandler(MinusApi sender, Exception e);

    public delegate void UploadItemCompleteHandler(MinusApi sender, UploadItemResult result);
    public delegate void UploadItemFailedHandler(MinusApi sender, Exception e);

    public delegate void SaveGalleryCompleteHandler(MinusApi sender);
    public delegate void SaveGalleryFailedHandler(MinusApi sender, Exception e);

    public delegate void GetItemsCompleteHandler(MinusApi sender, GetItemsResult result);
    public delegate void GetItemsFailedHandler(MinusApi sender, Exception e);

    public class MinusApi
    {
        #region Constants
        public static readonly String USER_AGENT = "MinusEngine_0.2";
        public static readonly Uri CREATE_GALLERY_URL = new Uri("http://min.us/api/CreateGallery");
        public static readonly Uri UPLOAD_ITEM_URL = new Uri("http://min.us/api/UploadItem");
        public static readonly Uri SAVE_GALLERY_URL = new Uri("http://min.us/api/SaveGallery");
        public static readonly String GET_ITEMS_URL = "http://min.us/api/GetItems/";
        #endregion

        #region Public fields
        public event CreateGalleryCompleteHandler CreateGalleryComplete;
        public event CreateGalleryFailedHandler CreateGalleryFailed;

        public event UploadItemCompleteHandler UploadItemComplete;
        public event UploadItemFailedHandler UploadItemFailed;

        public event SaveGalleryCompleteHandler SaveGalleryComplete;
        public event SaveGalleryFailedHandler SaveGalleryFailed;

        public event GetItemsCompleteHandler GetItemsComplete;
        public event GetItemsFailedHandler GetItemsFailed;

        public IWebProxy Proxy { get; set; }
        public String ApiKey { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="apiKey">The API Key assigned to your application.</param>
        public MinusApi(String apiKey)
        {
            // Just about as good as any other place to set this...
            System.Net.ServicePointManager.Expect100Continue = false;

            if (String.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("API key argument cannot be null");
            }

            this.ApiKey = apiKey;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Creates an empty new gallery.
        /// </summary>
        public void CreateGallery()
        {
            WebClient client = this.CreateAndSetupWebClient();
            client.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs e) {
                if (e.Error != null)
                {
                    Debug.WriteLine("CreateGallery operation failed: " + e.Error.Message);
                    this.TriggerCreateGalleryFailed(e.Error);
                    client.Dispose();
                    return;
                }

                CreateGalleryResult result = JsonConvert.DeserializeObject<CreateGalleryResult>(e.Result);
                Debug.WriteLine("CreateGallery operation successful: " + result);
                this.TriggerCreateGalleryComplete(result);
                client.Dispose();
            };

            try
            {
                ThreadPool.QueueUserWorkItem((object state) =>
                {
                    try
                    {
                        client.DownloadStringAsync(CREATE_GALLERY_URL);
                    }
                    catch (WebException e)
                    {
                        Debug.WriteLine("Failed to access CreateGallery API: " + e.Message);
                        this.TriggerCreateGalleryFailed(e);
                        client.Dispose();
                    }
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to submit task to thread pool: " + e.Message);
                this.TriggerCreateGalleryFailed(e);
                client.Dispose();
            }
        }

        /// <summary>
        /// Uploads an item (image) to a given gallery.
        /// 
        /// NOTE: This operations does NOT ensure that the item will be saved in the gallery.
        /// Saving is something that is done by the SaveGallery operation.
        /// Items that are uploaded are left in a transient state until they are saved via SaveGallery.
        /// </summary>
        /// <param name="editorId">Editor id of the gallery to which the item will be uploaded.</param>
        /// <param name="key">Key to the gallery.</param>
        /// <param name="filename">File location (full path) of the item to be uploaded.</param>
        /// <param name="desiredFilename">
        /// The desired filename for the item to be uploaded (defaults to null).
        /// If this parameter isn't provided, it will be taken from the filename param.
        /// Example:
        ///   filename is "C:\files\file.png" and you want it to be uploded as "image.png"
        /// </param>
        /// <returns>
        /// Cancellable operation. Since the operation is asynchronous and might take
        /// some time to complete, you can call cancel() on the returned Cancellable to
        /// abort this operation.
        /// </returns>
        public Cancellable UploadItem(String editorId, String key, String filename, String desiredFilename = null)
        {
            // Not worth checking for file existence or other stuff, as either Path.GetFileName or the upload
            // will check & fail
            String name = desiredFilename == null ? Path.GetFileName(filename) : desiredFilename;

            WebClient client = this.CreateAndSetupWebClient();
            client.QueryString.Add("editor_id", editorId);
            client.QueryString.Add("key", key);
            client.QueryString.Add("filename", UrlEncode(name));

            client.UploadFileCompleted += delegate(object sender, UploadFileCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    Debug.WriteLine("UploadItem operation failed: " + e.Error.Message);
                    this.TriggerUploadItemFailed(e.Error);
                    client.Dispose();
                    return;
                }

                String response = System.Text.Encoding.UTF8.GetString(e.Result);
                Trace.WriteLine(response);
                UploadItemResult result = JsonConvert.DeserializeObject<UploadItemResult>(response);
                Debug.WriteLine("UploadItem operation successful: " + result);
                this.TriggerUploadItemComplete(result);
                client.Dispose();
            };

            Cancellable cancellable = new CancellableAsyncUpload(client);

            try
            {
                ThreadPool.QueueUserWorkItem((object state) =>
                {
                    if (cancellable.IsCancelled())
                    {
                        Debug.WriteLine("Upload already cancelled!");
                        this.TriggerUploadItemFailed(new OperationCanceledException("Cancelled by request"));
                        client.Dispose();
                        return;
                    }

                    try
                    {
                        client.UploadFileAsync(UPLOAD_ITEM_URL, filename);
                    }
                    catch (WebException e)
                    {
                        Debug.WriteLine("Failed to access UploadItem API: " + e.Message);
                        this.TriggerUploadItemFailed(e);
                        client.Dispose();
                    }
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to submit task to thread pool: " + e.Message);
                this.TriggerUploadItemFailed(e);
                client.Dispose();
            }

            return cancellable;
        }

        /// <summary>
        /// Saves a gallery and makes it publicly accessible.
        /// </summary>
        /// <param name="name">Desired name for the gallery.</param>
        /// <param name="galleryEditorId">Gallery editor ID (obtained when created).</param>
        /// <param name="key">Editor key for the gallery (obtained when created).</param>
        /// <param name="items">
        /// The order in which the items will be displayed in the gallery.
        /// 
        /// If you fail to include items that were uploaded to this gallery, those items will be
        /// discarded by the server.
        /// </param>
        public void SaveGallery(String name, String galleryEditorId, String key, String[] items)
        {
            // Get a pre-configured web client
            WebClient client = this.CreateAndSetupWebClient();

            // build the item list (the order in which the items will be shown)
            StringBuilder builder = new StringBuilder().Append('[');
            for (int i = 0; i < items.Length; i++)
            {
                builder.Append("'").Append(items[i]).Append("',");
            }
            builder.Remove(builder.Length - 1, 1).Append(']');

            // Add the post data - must be as a string because WebClient doesn't do UrlEncode on all the
            // characters it's supposed to do. If I do UrlEncode() before submitting the webclient will
            // also perform url encoding on stuff that's already url encoded.
            StringBuilder data = new StringBuilder();
            data.Append("name=").Append(name)
            .Append("&id=").Append(galleryEditorId)
            .Append("&key=").Append(key)
            .Append("&items=").Append(UrlEncode(builder.ToString()));

            client.Headers["Content-Type"] = "application/x-www-form-urlencoded";

            // register the completion/error listener
            client.UploadStringCompleted += delegate(object sender, UploadStringCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    Debug.WriteLine("SaveGallery operation failed: " + e.Error.Message);
                    this.TriggerUploadItemFailed(e.Error);
                    client.Dispose();
                    return;
                }

                Debug.WriteLine("SaveGallery operation successful.");
                this.TriggerSaveGalleryComplete();
                client.Dispose();
            };

            // submit as an asynchronous task
            try
            {
                ThreadPool.QueueUserWorkItem((object state) =>
                {
                    try
                    {
                        client.UploadStringAsync(SAVE_GALLERY_URL, "POST", data.ToString());
                    }
                    catch (WebException e)
                    {
                        Debug.WriteLine("Failed to access SaveGallery API: " + e.Message);
                        this.TriggerSaveGalleryFailed(e);
                        client.Dispose();
                    }
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to submit task to thread pool: " + e.Message);
                this.TriggerSaveGalleryFailed(e);
                client.Dispose();
            }
        }

        /// <summary>
        /// Retrieve all items in a gallery, along with some other info (url and title).
        /// </summary>
        /// <param name="galleryReaderId">The reader id (public) of the gallery.</param>
        public void GetItems(String galleryReaderId)
        {
            if (String.IsNullOrEmpty(galleryReaderId))
            {
                throw new ArgumentException("Gallery Reader Id cannot be null or empty");
            }

            WebClient client = this.CreateAndSetupWebClient();
            client.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    Debug.WriteLine("GetItems operation failed: " + e.Error.Message);
                    this.TriggerGetItemsFailed(e.Error);
                    client.Dispose();
                    return;
                }

                GetItemsResult result = JsonConvert.DeserializeObject<GetItemsResult>(e.Result);
                Debug.WriteLine("GetItems operation successful: " + result);
                this.TriggerGetItemsComplete(result);
                client.Dispose();
            };

            try
            {
                ThreadPool.QueueUserWorkItem((object state) =>
                {
                    try
                    {
                        client.DownloadStringAsync(new Uri(GET_ITEMS_URL + galleryReaderId));
                    }
                    catch (WebException e)
                    {
                        Debug.WriteLine("Failed to access GetItems API: " + e.Message);
                        this.TriggerGetItemsFailed(e);
                        client.Dispose();
                    }
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to submit task to thread pool: " + e.Message);
                this.TriggerGetItemsFailed(e);
                client.Dispose();
            }
        }

        #endregion

        #region Public helpers
        /// <summary>
        /// Perform URL escaping on a string.
        /// </summary>
        /// <param name="parameter">Input (unescaped) string.</param>
        /// <returns>Escaped string.</returns>
        public static String UrlEncode(String parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                return string.Empty;
            }

            String value = Uri.EscapeDataString(parameter);

            // Uri.EscapeDataString escapes with lowercase characters, convert to uppercase
            value = Regex.Replace(value, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper());

            // not escaped by Uri.EscapeDataString() but need to be escaped
            value = value
                .Replace("(", "%28")
                .Replace(")", "%29")
                .Replace("$", "%24")
                .Replace("!", "%21")
                .Replace("*", "%2A")
                .Replace("'", "%27");

            // characters escaped by Uri.EscapeDataString() that need to be sent unescaped
            value = value.Replace("%7E", "~");

            return value;
        }
        #endregion

        #region Private helpers

        private WebClient CreateAndSetupWebClient()
        {
            WebClient client = new WebClient();
            if (this.Proxy != null)
            {
                client.Proxy = this.Proxy;
            }
            client.Headers["User-Agent"] = USER_AGENT;
            return client;
        }
        #endregion

        #region Event Triggering
        private void TriggerCreateGalleryComplete(CreateGalleryResult result)
        {
            if (this.CreateGalleryComplete != null)
            {
                this.CreateGalleryComplete.Invoke(this, result);
            }
        }

        private void TriggerCreateGalleryFailed(Exception e)
        {
            if (this.CreateGalleryFailed != null)
            {
                this.CreateGalleryFailed.Invoke(this, e);
            }
        }

        private void TriggerUploadItemComplete(UploadItemResult result)
        {
            if (this.UploadItemComplete != null)
            {
                this.UploadItemComplete.Invoke(this, result);
            }
        }

        private void TriggerUploadItemFailed(Exception e)
        {
            if (this.UploadItemFailed != null)
            {
                this.UploadItemFailed.Invoke(this, e);
            }
        }

        private void TriggerSaveGalleryComplete()
        {
            if (this.SaveGalleryComplete != null)
            {
                this.SaveGalleryComplete.Invoke(this);
            }
        }

        private void TriggerSaveGalleryFailed(Exception e)
        {
            if (this.SaveGalleryFailed != null)
            {
                this.SaveGalleryFailed.Invoke(this, e);
            }
        }

        private void TriggerGetItemsComplete(GetItemsResult result)
        {
            if (this.GetItemsComplete != null)
            {
                this.GetItemsComplete.Invoke(this, result);
            }
        }

        private void TriggerGetItemsFailed(Exception e)
        {
            if (this.GetItemsFailed != null)
            {
                this.GetItemsFailed.Invoke(this, e);
            }
        }
        #endregion
    }
}
