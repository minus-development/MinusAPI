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
using System.Net;

namespace BiasedBit.MinusEngine
{
    public interface Cancellable
    {
        Boolean Cancel();

        Boolean IsCancelled();
    }

    public class CancellableAsyncUpload : Cancellable
    {
        #region Private fields
        private WebClient client;
        private Boolean cancelled;
        #endregion

        #region Constructors
        public CancellableAsyncUpload(WebClient client)
        {
            this.client = client;
        }
        #endregion

        #region Public methods
        public Boolean Cancel()
        {
            lock (this)
            {
                if (this.cancelled)
                {
                    return false;
                }

                if (client != null)
                {
                    this.cancelled = true;
                }

                if (client.IsBusy)
                {
                    client.CancelAsync();
                }

                return true;
            }
        }

        public Boolean IsCancelled()
        {
            lock (this)
            {
                return this.cancelled;
            }
        }
        #endregion
    }
}
