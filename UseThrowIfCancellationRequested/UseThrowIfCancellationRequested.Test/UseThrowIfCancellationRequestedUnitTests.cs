using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using TestHelper;
using UseThrowIfCancellationRequested;

namespace UseThrowIfCancellationRequested.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        [TestMethod]
        public void EmptyText_NoDiagnostics()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }
        [TestMethod]
        public void NoThrow_NoDiagnostic()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public async Task Test(CancellationToken ct)
            {
                if(ct.IsCancellationRequested)
                    Console.ReadKey();
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void NoThrowBlock_NoDiagnostic()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public async Task Test(CancellationToken ct)
            {
                if(ct.IsCancellationRequested)
                {
                    Console.ReadKey();
                }
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }
        [TestMethod]
        public void SingleDiagnostic()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public async Task Test(CancellationToken ct)
            {
                if(ct.IsCancellationRequested)
                    throw new OperationCancelledException();
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "UseThrowIfCancellationRequested",
                Message = String.Format("Replace if statement with ThrowIfCancellationRequested()"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void SingleFix()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public async Task Test(CancellationToken ct)
            {
                if(ct.IsCancellationRequested)
                    throw new OperationCancelledException();
            }
        }
    }";

            var fixtest = @"using System;
using System.Threading.Tasks;
using System.Threading;

namespace ConsoleApplication1
{
    class TypeName
    {
        public async Task Test(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }
        public async Task Test(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
        }
        [TestMethod]
        public void SingleFix_IfBlock()
        {
            var test = @"
    using System;
    using System.Threading.Tasks;
    using System.Threading;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public async Task Test(CancellationToken ct)
            {
                if(ct.IsCancellationRequested)
                {
                    throw new OperationCancelledException();
                }
            }
        }
    }";

            var fixtest = @"using System;
using System.Threading.Tasks;
using System.Threading;

namespace ConsoleApplication1
{
    class TypeName
    {
        public async Task Test(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
        }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new UseThrowIfCancellationRequestedCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new UseThrowIfCancellationRequestedAnalyzer();
        }
    }
}
