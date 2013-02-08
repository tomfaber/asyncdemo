/* Copyright 2013 Tom Faber

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.  */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AsyncDemo.Implementations
{
    // For a very elegant, functionally complete, and performant implementation of IAsyncResult, download the sample
    // code at http://msdn.microsoft.com/en-us/magazine/cc163467.aspx.  For an implementation that gets the job
    // done to the extent needed by this demo, read on:
    public class CustomAsyncResult : IAsyncResult
    {
        public object AsyncState
        {
            get;
            private set;
        }

        private ManualResetEvent _completed = new ManualResetEvent(false);
        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get { return _completed; }
        }

        public bool CompletedSynchronously
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsCompleted
        {
            get { throw new NotImplementedException(); }
        }

        internal void Complete()
        {
            _completed.Set();
        }

        public CustomAsyncResult(object asyncState)
        {
            this.AsyncState = asyncState;
        }
    }
}
