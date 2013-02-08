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
using System.Threading.Tasks;

namespace AsyncDemo.Implementations
{
    /// <summary>
    /// Shows how great .NET 4.5 is!  All the business logic is in one method, PublicApiAsync. This is still 
    /// a sandwich - you need to translate from APM to Tasks and back again.
    /// </summary>
    public class AsyncAwait : IWebService
    {
        private ApmToTaskAdapter _taskProvider;

        public AsyncAwait(MockAsyncThing mat)
        {
            _taskProvider = new ApmToTaskAdapter(mat);
        }

        public IAsyncResult BeginPublicApi(AsyncCallback callback, object asyncState)
        {
            Task<int> t = PublicApiAsync();

            // the next few lines of code link the async/await pattern with the APM,
            //  ensuring that the asyncState and callback are handled correctly.
            //  for more robust implementations, see https://gist.github.com/AArnott/4608941
            //  and http://blogs.msdn.com/b/pfxteam/archive/2011/06/27/10179452.aspx.

            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>(asyncState);
            t.ContinueWith((t2) =>
                {
                    tcs.SetResult(t2.Result);
                    if (callback != null) callback(tcs.Task);
                });
            return tcs.Task;
        }

        private async Task<int> PublicApiAsync()
        {
            int a = await _taskProvider.AAsync();
            Task<int> b = _taskProvider.BAsync(a);
            Task<int> c = _taskProvider.CAsync(a);
            await Task.WhenAll(b, c);
            if (b.Result > c.Result)
            {
                return await _taskProvider.DAsync(b.Result, c.Result);
            }
            else
            {
                return c.Result;
            }
        }
        
        public int EndPublicApi(IAsyncResult asyncResult)
        {
            return ((Task<int>)asyncResult).Result;
        }
    }
}
