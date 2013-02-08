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
    class ApmToTaskAdapter
    {
        private MockAsyncThing _mat;

        public ApmToTaskAdapter(MockAsyncThing mat)
        {
            _mat = mat;
        }

        internal Task<int> AAsync()
        {
            return Task.Factory.FromAsync<int>(_mat.BeginA, _mat.EndA, TaskCreationOptions.None);
        }

        internal Task<int> BAsync(int a)
        {
            return Task.Factory.FromAsync<int, int>(_mat.BeginB, _mat.EndB, a, TaskCreationOptions.None);
        }

        internal Task<int> CAsync(int a)
        {
            return Task.Factory.FromAsync<int, int>(_mat.BeginC, _mat.EndC, a, TaskCreationOptions.None);
        }

        internal Task<int> DAsync(int p1, int p2)
        {
            return Task.Factory.FromAsync<int, int, int>(_mat.BeginD, _mat.EndD, p1, p2, TaskCreationOptions.None);
        }
    }
}