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
using System.Threading.Tasks;

namespace AsyncDemo.Implementations
{
    /// <summary>
    /// Here's the sandwich, where you translate from APM to Tasks and back to APM.  This is probably more
    /// complex than it's worth, however if you have specific reasons to use Tasks (for example, if this
    /// is the first step in a transition to .NET 4.5 and async/await - or for another example if you need
    /// cancellation support), then here's how it can work.  Note that ApmToTaskAdapter is shared between
    /// this implementation and the async/await implementation.
    /// </summary>
    public class TaskWrapper : IWebService
    {
        private MockAsyncThing _mat;
        private ApmToTaskAdapter _matAdapter;
        private Task<int> _taskA;
        private int _finalResult;

        public TaskWrapper(MockAsyncThing mat)
        {
            _mat = mat;
            _matAdapter = new ApmToTaskAdapter(mat);
        }

        public IAsyncResult BeginPublicApi(AsyncCallback callback, object asyncState)
        {
            // TODO: figure out if there's a way I can return a task instead of a CustomAsyncResult, which
            //   would be more in keeping with the theme of this implementation, and show something
            //   different from the AnonymousDelegates and CallbackSeparation implementations.
            //   set a task that is the parent of all of the individual tasks?
            //   use TaskCompletionSource?

            CustomAsyncResult car = new CustomAsyncResult(asyncState);
            _taskA = _matAdapter.AAsync();
            _taskA.ContinueWith((taskA) =>
            {
                Task<int> _taskB = _matAdapter.BAsync(_taskA.Result);
                Task<int> _taskC = _matAdapter.CAsync(_taskA.Result);
                Task.WhenAll(_taskB,_taskC).ContinueWith((aggregateTask) =>
                    {
                        if (_taskB.Result > _taskC.Result)
                        {
                            Task<int> _taskD = _matAdapter.DAsync(_taskB.Result, _taskC.Result);
                            _taskD.ContinueWith((taskD) =>
                                {
                                    _finalResult = taskD.Result;
                                    car.Complete();
                                    if (callback != null) callback(car);
                                });
                        }
                        else
                        {
                            _finalResult = _taskC.Result;
                            car.Complete();
                            if (callback != null) callback(car);
                        }
                    });
            });
            // I don't have to call Start() on the tasks, it automatically calls Begin when you create the task with FromAsync.
            return car;
        }

        public int EndPublicApi(IAsyncResult asyncResult)
        {
            return _finalResult;
        }
    }
}
