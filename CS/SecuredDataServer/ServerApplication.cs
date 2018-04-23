using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.ExpressApp.MiddleTier;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using XafModule;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;

namespace DataServer {
    public class ExampleServerApplication : ServerApplication {
        public ExampleServerApplication() {
            ApplicationName = "ServerUsageExample";
            this.Modules.Add(new ExampleModule());
        }
        protected override void CreateDefaultObjectSpaceProvider(CreateCustomObjectSpaceProviderEventArgs args) {
            args.ObjectSpaceProvider = new XPObjectSpaceProvider(args.ConnectionString, args.Connection);
        }
    }
}
