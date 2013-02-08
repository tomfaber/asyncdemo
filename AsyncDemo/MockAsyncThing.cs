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
using System.IO;
using System.Net;
using System.Threading;

namespace AsyncDemo
{
    /// <summary>
    /// Call each one only once.  Methods on this class are not thread-safe, if you call the same method
    /// multiple times from different threads it may get into an inconsistent state.
    /// </summary>
    /// <remarks>Uses file I/O with a temp file so that the "Async-ness" of it all is real.</remarks>
    public class MockAsyncThing : IDisposable
    {
        string filePath = Environment.ExpandEnvironmentVariables("%TEMP%\\MockAsyncThing");

        public MockAsyncThing()
        {
            if (!File.Exists(filePath))
            {
                using (File.Create(filePath));
            }
            for (int i = 0; i < numberOfOperations; i++)
            {
                results[i] = r.Next(100);
            }
        }

        const int numberOfOperations = 4;
        private readonly List<string> Errors = new List<string>();

        private int aBegun = 0;
        private int aEnded = 0;
        private int bBegun = 0;
        private int bEnded = 0;
        private int cBegun = 0;
        private int cEnded = 0;
        private int dBegun = 0;
        private int dEnded = 0;

        private static Random r = new Random();
        private FileStream[] requests = new FileStream[numberOfOperations];
        private int[] results = new int[numberOfOperations];

        public IAsyncResult BeginA(AsyncCallback callback, object asyncState)
        {
            CheckBegin("A", ref aBegun);
            return InnerBegin(0, callback, asyncState);
        }

        public int EndA(IAsyncResult asyncResult)
        {
            CheckEnd("A", ref aEnded);
            return InnerEnd(0, asyncResult);
        }

        public IAsyncResult BeginB(int aResult, AsyncCallback callback, object asyncState)
        {
            if (aEnded < 1 || aResult != results[0])
            {
                Error("B called without correct input from A");
            }
            CheckBegin("B", ref bBegun);
            return InnerBegin(1, callback, asyncState);
        }

        public int EndB(IAsyncResult asyncResult)
        {
            CheckEnd("B", ref bEnded);
            return InnerEnd(1, asyncResult);
        }

        public IAsyncResult BeginC(int aResult, AsyncCallback callback, object asyncState)
        {
            if (aEnded < 1 || aResult != results[0])
            {
                Error("C called without correct input from A");
            }
            CheckBegin("C", ref cBegun);
            return InnerBegin(2, callback, asyncState);
        }

        public int EndC(IAsyncResult asyncResult)
        {
            CheckEnd("C", ref cEnded);
            return InnerEnd(2, asyncResult);
        }

        public IAsyncResult BeginD(int bResult, int cResult, AsyncCallback callback, object asyncState)
        {
            if (bEnded < 1 || bResult != results[1])
            {
                Error("D called without correct input from B");
            }
            if (cEnded < 1 || cResult != results[2])
            {
                Error("D called without correct input from C");
            }
            CheckBegin("D", ref dBegun);
            return InnerBegin(3, callback, asyncState);
        }

        public int EndD(IAsyncResult asyncResult)
        {
            CheckEnd("D", ref dEnded);
            return InnerEnd(3, asyncResult);
        }

        private void CheckBegin(string name, ref int begunCounter)
        {
            if (Interlocked.Increment(ref begunCounter) > 1)
            {
                throw new AsyncDemoValidationException(name + " begin called more than once");
            }
        }

        private void CheckEnd(string name, ref int endedCounter)
        {
            if (Interlocked.Increment(ref endedCounter) > 1)
            {
                Error(name + " ended more than once");
            }
        }

        private int InnerEnd(int operation, IAsyncResult asyncResult)
        {
            try
            {
                requests[operation].EndRead(asyncResult);
            }
            finally
            {
                requests[operation].Dispose();
            }

            return results[operation];
        }

        private IAsyncResult InnerBegin(int operation, AsyncCallback callback, object asyncState)
        {
            var fs = File.OpenRead(filePath);

            requests[operation] = fs;
            return fs.BeginRead(new byte[0], 0, 0, callback, asyncState);
        }

        /// <summary>
        /// /// Throws if you did not do all of these things exactly once each, and in this order: 
        /// 1) call A
        /// 2) call B and C, passing in result from A.
        /// 3) if B > C call D passing in both results
        /// </summary>
        public void Validate()
        {
            Validate(true);
        }

        /// <summary>
        /// Throws if you did not do these things exactly once each, and in this order:
        /// 1) call A
        /// 2) call B and C, passing in result from A.
        /// 3) if B > C call D passing in both results
        /// </summary>
        /// <param name="mustBeComplete">If false, allows you to have started but not completed the sequence.</param>
        public void Validate(bool mustBeComplete)
        {
            if (Errors.Count > 0)
            {
                throw new AsyncDemoValidationException(string.Join("; ", Errors));
            }
            if (mustBeComplete)
            {
                if (aBegun != 1 ||
                    aEnded != 1 ||
                    bBegun != 1 ||
                    bEnded != 1 ||
                    cBegun != 1 ||
                    cEnded != 1)
                {
                    throw new AsyncDemoValidationException("not all operations were called and completed: " + string.Join(" ", new[] { aBegun, aEnded, bBegun, bEnded, cBegun, cEnded }));
                }
                if (DShouldBeCalled)
                {
                    if (dBegun != 1 || dEnded != 1)
                    {
                        throw new AsyncDemoValidationException("D was not called and completed");
                    }
                }
                else
                {
                    if (dBegun != 0 || dEnded != 0)
                    {
                        throw new AsyncDemoValidationException("D called but should not have been");
                    }
                }
            }
        }

        private void Error(string errorMessage)
        {
            Errors.Add(errorMessage);
        }

        public bool DShouldBeCalled { 
            get
            {
                return results[1] > results[2];
            }
            set
            {
                results[1] = r.Next(0, 10000);
                results[2] = results[1] + (value ? -1 : 1);
            }
        }

        #region Disposable

        private bool _disposed;
        private object _disposalSync = new object();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                lock (_disposalSync)
                {
                    if (!_disposed)
                    {
                        for (int i = 0; i < requests.Length; i++)
                        {
                            if (requests[i] != null)
                            {
                                requests[i].Dispose();
                                requests[i] = null;
                            }
                        }
                        _disposed = true;
                    }
                }
            }
        }

        #endregion

        public int ExpectedResult
        {
            get
            {
                return (results[1] > results[2] ? results[3] : results[2]);
            }
        }
    }
}