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
using System.Threading;

namespace AsyncDemo.Implementations
{
    /// <summary>
    /// Implements APM to APM, using anonymous delegates for the asynchronous callbacks. This makes for more
    /// compact code, and keeps the overall logic in fewer methods, but the drawback is that as the logic gets
    /// more complex, the method(s) get quite long.  Also, if your code reviewers aren't comfortable with
    /// delegates this will be a headache for them to keep straight what is going on when.
    /// </summary>
    public class AnonymousDelegates : IWebService
    {
        private MockAsyncThing _mat;
        private CustomAsyncResult _asyncResult;
        private int _finalResultValue;
        private int _bResultValue;
        private int _cResultValue;
        private int _bAndCCalled = 0;
        private AsyncCallback _userCallback;

        public AnonymousDelegates(MockAsyncThing mat)
        {
            _mat = mat;
        }

        public IAsyncResult BeginPublicApi(AsyncCallback callback, object asyncState)
        {
            _asyncResult = new CustomAsyncResult(asyncState);
            _userCallback = callback;
            _mat.BeginA((iar) => 
                {
                    int aResultValue = _mat.EndA(iar);
                    ManualResetEvent bResultObtained = new ManualResetEvent(false);
                    _mat.BeginB(aResultValue, (iarB) => 
                        {
                            _bResultValue = _mat.EndB(iarB);
                            ProcessResultsFromBAndC();
                        }, null);
                    _mat.BeginC(aResultValue,(iarC) =>
                        {
                            _cResultValue = _mat.EndC(iarC);
                            ProcessResultsFromBAndC();
                        },null);
                }, null);

            return _asyncResult;
        }

        private void ProcessResultsFromBAndC()
        {
            if (Interlocked.Increment(ref _bAndCCalled) == 2)
            {
                if (_bResultValue > _cResultValue)
                {
                    _mat.BeginD(_bResultValue, _cResultValue, (iarD) =>
                        {
                            SetComplete(_mat.EndD(iarD));
                        }, null);
                }
                else
                {
                    int finalResult = _cResultValue;
                    SetComplete(finalResult);
                }
            }
        }

        private void SetComplete(int finalResult)
        {
            _finalResultValue = finalResult;
            _asyncResult.Complete();
            if (_userCallback != null) _userCallback(_asyncResult);
        }

        public int EndPublicApi(IAsyncResult asyncResult)
        {
            return _finalResultValue;
        }
    }
}