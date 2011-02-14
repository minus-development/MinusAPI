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
using BiasedBit.MinusEngine;
using System.Threading;

namespace BiasedBit.MinusEngineTestApp
{
    class Program
    {
        private static readonly String API_KEY = "dummyKey";

        static void Main(string[] args)
        {
            // This whole API is made to be completely asynchronous.
            // There are no blocking calls, hence the usage of event handling delegates.
            // Looks ugly in this console example but it's perfect for UI based applications.

            // Warning: The API KEY feature is still not implemented in Minus, so just pass in some "dummyKey"

            // Pick one of these methods below to test the features independently
            TestGetItems();
            //TestAll();

            // Sleep a bit so you can check the output...
            Thread.Sleep(40000);
        }

        /// <summary>
        /// This method
        /// </summary>
        private static void TestGetItems()
        {
            // create the API
            MinusApi api = new MinusApi(API_KEY);

            // setup the success handler for GetItems operation
            api.GetItemsComplete += delegate(MinusApi sender, GetItemsResult result)
            {
                Console.WriteLine("Gallery items successfully retrieved!\n---");
                Console.WriteLine("Read-only URL: " + result.ReadonlyUrl);
                Console.WriteLine("Title: " + result.Title);
                Console.WriteLine("Items:");
                foreach (String item in result.Items)
                {
                    Console.WriteLine(" - " + item);
                }
            };

            // setup the failure handler for the GetItems operation
            api.GetItemsFailed += delegate(MinusApi sender, Exception e)
            {
                // don't do anything else...
                Console.WriteLine("Failed to get items from gallery...\n" + e.Message);
            };

            // trigger the GetItems operation - notice the extra "m" in there.
            // while the REAL reader id is "vgkRZC", the API requires you to put the extra "m" in there
            api.GetItems("mvgkRZC");
        }

        /// <summary>
        /// Tests the full scope of methods in the API.
        /// This method creates a gallery, uploads a couple of items, saves them and then retrieves them.
        /// Make sure you change the values of the items in the "items" array to match actually valid files or this
        /// will fail.
        /// </summary>
        private static void TestAll()
        {
            // The call that triggers the program is the near the end of this method
            // (the rest is pretty much setup to react to events)

            // create the API
            MinusApi api = new MinusApi(API_KEY);

            // Prepare the items to be uploaded
            String[] items =
            {
                @"C:\Users\bruno\Desktop\clown.png",
                @"C:\Users\bruno\Desktop\small.png"
            };
            IList<String> uploadedItems = new List<String>(items.Length);

            // create a couple of things we're going to need between requests
            CreateGalleryResult galleryCreated = null;

            // set up the listeners for CREATE
            api.CreateGalleryFailed += delegate(MinusApi sender, Exception e)
            {
                // don't do anything else...
                Console.WriteLine("Failed to create gallery..." + e.Message);
            };
            api.CreateGalleryComplete += delegate(MinusApi sender, CreateGalleryResult result)
            {
                // gallery created, trigger upload of the first file
                galleryCreated = result;
                Console.WriteLine("Gallery created! " + result);
                Thread.Sleep(1000);
                Console.WriteLine("Uploading files...");
                api.UploadItem(result.EditorId, result.Key, items[0]);
            };

            // set up the listeners for UPLOAD
            api.UploadItemFailed += delegate(MinusApi sender, Exception e)
            {
                // don't do anything else...
                Console.WriteLine("Upload failed: " + e.Message);
            };
            api.UploadItemComplete += delegate(MinusApi sender, UploadItemResult result)
            {
                // upload complete, either trigger another upload or save the gallery if all files have been uploaded
                Console.WriteLine("Upload successful: " + result);
                uploadedItems.Add(result.Id);
                if (uploadedItems.Count == items.Length)
                {
                    // if all the elements are uploaded, then save the gallery
                    Console.WriteLine("All uploads complete, saving gallery...");
                    api.SaveGallery("testGallery", galleryCreated.EditorId, galleryCreated.Key, uploadedItems.ToArray());
                }
                else
                {
                    // otherwise just keep uploading
                    Console.WriteLine("Uploading item " + (uploadedItems.Count + 1));
                    api.UploadItem(galleryCreated.EditorId, galleryCreated.Key, items[uploadedItems.Count]);
                }
            };

            // set up the listeners for SAVE
            api.SaveGalleryFailed += delegate(MinusApi sender, Exception e)
            {
                Console.WriteLine("Failed to save gallery... " + e.Message);
            };
            api.SaveGalleryComplete += delegate(MinusApi sender)
            {
                // The extra "m" is appended because minus uses the first character to determine the type of data
                // you're accessing (image, gallery, etc) and route you accordingly.
                Console.WriteLine("Gallery saved! You can now access it at http://min.us/m" + galleryCreated.ReaderId);
            };

            // this is the call that actually triggers the whole program
            api.CreateGallery();
        }
    }
}
