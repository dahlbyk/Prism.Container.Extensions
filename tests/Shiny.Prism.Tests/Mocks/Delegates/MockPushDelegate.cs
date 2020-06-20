﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shiny.Push;

namespace Shiny.Prism.Mocks.Delegates
{
    public class MockPushDelegate : IPushDelegate
    {
        public Task OnEntry(PushEntryArgs args)
        {
            throw new NotImplementedException();
        }

        public Task OnReceived(IDictionary<string, string> data)
        {
            throw new NotImplementedException();
        }

        public Task OnTokenChanged(string token)
        {
            throw new NotImplementedException();
        }
    }
}
