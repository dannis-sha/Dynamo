using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoFFI;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoFFITests
{
    public abstract class FFITestSetup
    {
        public ProtoCore.Core Setup()
        {
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();
            return core;
        }
        protected struct ValidationData
        {
            public string ValueName;
            public dynamic ExpectedValue;
            public int BlockIndex;
        }
        protected int ExecuteAndVerify(String code, ValidationData[] data, Dictionary<string, Object> context)
        {
            int errors = 0;
            return ExecuteAndVerify(code, data, context, out errors);
        }
        protected int ExecuteAndVerify(String code, ValidationData[] data)
        {
            int errors = 0;
            return ExecuteAndVerify(code, data, out errors);
        }
        protected int ExecuteAndVerify(String code, ValidationData[] data, out int nErrors)
        {
            return ExecuteAndVerify(code, data, null, out nErrors);
        }
        protected int ExecuteAndVerify(String code, ValidationData[] data, Dictionary<string, Object> context, out int nErrors)
        {
            ProtoCore.Core core = Setup();
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core, context);
            int nWarnings = core.RuntimeStatus.Warnings.Count;
            nErrors = core.BuildStatus.ErrorCount;
            if (data == null)
            {
                core.Cleanup();
                return nWarnings + nErrors;
            }
            TestFrameWork thisTest = new TestFrameWork();
            foreach (var item in data)
            {
                if (item.ExpectedValue == null)
                {
                    object nullOb = null;
                    TestFrameWork.Verify(mirror, item.ValueName, nullOb, item.BlockIndex);
                }
                else
                {
                    TestFrameWork.Verify(mirror, item.ValueName, item.ExpectedValue, item.BlockIndex);
                }
            }
            core.Cleanup();
            return nWarnings + nErrors;
        }
    }
    public class CSFFITest : FFITestSetup
    {
        /*
[Test]
        TestFrameWork thisTest = new TestFrameWork();

        [Test]
        public void TestImportDummyClass()
        {
            String code =
            @"              
            Type dummy = Type.GetType("ProtoFFITests.Dummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestImportAllClasses()
        {
            String code =
            @"import(""ProtoGeometry.dll"");
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 },
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestImportPointClass()
        {
            String code =
            @"import(Point from ""ProtoGeometry.dll"");
            ValidationData[] data = { new ValidationData { ValueName="x", ExpectedValue = 1.0, BlockIndex = 0},
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestImportPointClassWithoutImportingVectorClass()
        {
            String code =
            @"
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("Method Resolution")]
        public void TestInstanceMethodResolution()
        {
            String code =
            @"
            ValidationData[] data =
                {
                    new ValidationData { ValueName = "o", ExpectedValue = 3, BlockIndex = 0 },
                    new ValidationData { ValueName = "o2", ExpectedValue = 4, BlockIndex = 0 }

                };

            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestProperty()
        {
            String code =
           @"
           ValidationData[] data =
               {
                   new ValidationData { ValueName = "v", ExpectedValue = 1, BlockIndex = 0 },
                   new ValidationData { ValueName = "t", ExpectedValue = false, BlockIndex = 0 }

               };
           ExecuteAndVerify(code, data);

        }

        [Test]
        public void TestStaticProperty()
        {
            String code =
           @"

            ValidationData[] data =
               {
                   new ValidationData { ValueName = "v", ExpectedValue = 42, BlockIndex = 0 },
                   new ValidationData { ValueName = "s", ExpectedValue = 42, BlockIndex = 0 },
                   new ValidationData { ValueName = "t", ExpectedValue = 42, BlockIndex = 0 }

               };
            
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestImportVectorAndPointClass()
        {
            String code =
            @"
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestArrayMarshling_MixedTypes()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.DerivedDummy");
            Type derived1 = Type.GetType("ProtoFFITests.Derived1");
            Type testdispose = Type.GetType("ProtoFFITests.TestDispose");
            Type dummydispose = Type.GetType("ProtoFFITests.DummyDispose");
            code = string.Format("import(\"{0}\");\r\nimport(\"{1}\");\r\nimport(\"{2}\");\r\nimport(\"{3}\");\r\n{4}",
                dummy.AssemblyQualifiedName, derived1.AssemblyQualifiedName, testdispose.AssemblyQualifiedName, dummydispose.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 128.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestDisposeOnImplicitImport()
        {
            String code =
            @"
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("Method Resolution")]
        public void TestArrayElementReturnedFromFunction()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.DerivedDummy");
            Type derived1 = Type.GetType("ProtoFFITests.Derived1");
            Type testdispose = Type.GetType("ProtoFFITests.TestDispose");
            Type dummydispose = Type.GetType("ProtoFFITests.DummyDispose");
            code = string.Format("import(\"{0}\");\r\nimport(\"{1}\");\r\nimport(\"{2}\");\r\nimport(\"{3}\");\r\n{4}",
                dummy.AssemblyQualifiedName, derived1.AssemblyQualifiedName, testdispose.AssemblyQualifiedName, dummydispose.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = 128.0, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestArrayMarshalling_DStoCS()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.Dummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 55.0, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestArrayMarshalling_DStoCS_CStoDS()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.Dummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 110.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestListMarshalling_DStoCS()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.Dummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 55.0, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestStackMarshalling_DStoCS_CStoDS()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.DerivedDummy");
            Type derived1 = Type.GetType("ProtoFFITests.Derived1");
            Type testdispose = Type.GetType("ProtoFFITests.TestDispose");
            Type dummydispose = Type.GetType("ProtoFFITests.DummyDispose");
            code = string.Format("import(\"{0}\");\r\nimport(\"{1}\");\r\nimport(\"{2}\");\r\nimport(\"{3}\");\r\n{4}",
                dummy.AssemblyQualifiedName, derived1.AssemblyQualifiedName, testdispose.AssemblyQualifiedName, dummydispose.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "size", ExpectedValue = 3, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestDictionaryMarshalling_DStoCS_CStoDS()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.Dummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 45, BlockIndex = 1 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestListMarshalling_DStoCS_CStoDS()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.Dummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 55.0, BlockIndex = 0 } };
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }

        [Test]
        public void TestInheritanceImport()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.DerivedDummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num123", ExpectedValue = (Int64)123, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestMethodCallOnNullFFIObject()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.Dummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = null, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestStaticMethod()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.Dummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "value", ExpectedValue = (Int64)100, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceBaseClassMethodCall()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.DerivedDummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 55.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceVirtualMethodCallDerived()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.DerivedDummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "isBase", ExpectedValue = false, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceVirtualMethodCallBase()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.DerivedDummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "isBase", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceCtorsVirtualMethods()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.Derived1");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            Console.WriteLine(code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 10.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceCtorsVirtualMethods2()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.Derived1");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 20.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceCtorsVirtualMethods3()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.Derived1");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "num", ExpectedValue = 20.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestInheritanceAcrossLangauges_CS_DS()
        {
            string code = @"
            ValidationData[] data = { new ValidationData { ValueName = "x", ExpectedValue = Math.Sqrt(3.0), BlockIndex = 0 } };
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () => ExecuteAndVerify(code, data));
        }
        /// <summary>
        /// This is to test Dispose method on IDisposable object. Dispose method 
        /// on IDisposable is renamed to _Dispose as DS destructor. Calling 
        /// Dispose doesn't affect the state.
        /// </summary>

        [Test]
        public void TestDisposeNotAvailable()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.TestDispose");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)15, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }
        /// <summary>
        /// This is to test Dispose method on IDisposable object. Dispose method 
        /// on IDisposable is renamed to _Dispose as DS destructor. Calling 
        /// _Dispose will make the object a null.
        /// </summary>

        [Test]
        public void TestDisposable_Dispose()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.TestDispose");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = null, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }
        /// <summary>
        /// This is to test _Dispose method is added to all the classes.
        /// If object is IDisposable Dispose will be renamed to _Dispose
        /// </summary>

        [Test]
        public void TestDummyBase_Dispose()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.DummyBase");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = null, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }
        /// <summary>
        /// This is to test Dispose method is not renamed to _Dispose if the
        /// object is not IDisposable. Calling Dispose doesn't invalidate the
        /// object and value is set to -2, in Dispose implementation.
        /// </summary>

        [Test]
        public void TestDummyDisposeDispose()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.DummyDispose");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)(-2), BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }
        /// <summary>
        /// This test is to test for dispose due to update. When the same 
        /// variable is re-initialized, the previous instance will be disposed
        /// and this pointer will be re-used for next instance. Test if object
        /// reference is properly cleaned from CLRObjectMarshaler and this works
        /// without an issue.
        /// </summary>

        [Test]
        public void TestDisposeForUpdate()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.DummyDispose");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)(20), BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }
        /// <summary>
        /// This test is to test for dispose due to update. When the same 
        /// variable is re-initialized, the previous instance will be disposed
        /// and this pointer will be re-used for next instance. Test if object
        /// reference is properly cleaned from CLRObjectMarshaler and this works
        /// without an issue.
        /// </summary>

        [Test]
        public void TestDisposeForUpdate2()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.DummyDispose");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)(20), BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }
        /// <summary>
        /// This is to test Dispose method renamed to _Dispose if the object
        /// is derived from IDisposable object. Calling _Dispose will make 
        /// the object null.
        /// </summary>

        [Test]
        public void TestDisposeDerived_Dispose()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.TestDisposeDerived");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = null, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }
        /// <summary>
        /// This is to test import of multiple dlls.
        /// </summary>

        [Test]
        public void TestMultipleImport()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.DerivedDummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 7260.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }
        /// <summary>
        /// This is to test import of multiple dlls.
        /// </summary>

        [Test]
        public void TestImportSameModuleMoreThanOnce()
        {
            String code =
            @"
            Type dummy = Type.GetType("ProtoFFITests.DerivedDummy");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            Type dummy2 = Type.GetType("ProtoFFITests.DerivedDummy");
            Type derived1 = Type.GetType("ProtoFFITests.Derived1");
            Type testdispose = Type.GetType("ProtoFFITests.TestDispose");
            Type dummydispose = Type.GetType("ProtoFFITests.DummyDispose");
            code = string.Format("import(\"{0}\");\r\nimport(\"{1}\");\r\nimport(\"{2}\");\r\nimport(\"{3}\");\r\n{4}",
                dummy2.AssemblyQualifiedName, derived1.AssemblyQualifiedName, testdispose.AssemblyQualifiedName, dummydispose.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "sum", ExpectedValue = 7260.0, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestPropertyAccessor()
        {
            String code =
            @"
            double aa = 1;
            ValidationData[] data = { new ValidationData { ValueName = "a", ExpectedValue = aa, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestAssignmentSingleton()
        {
            String code =
            @"
            object[] aa = new object[] { 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a", ExpectedValue = aa, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestAssignmentAsArray()
        {
            String code =
            @"
            object[] aa = new object[] { 1.0, 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a", ExpectedValue = aa, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestReturnFromFunctionSingle()
        {
            String code =
            @"
            var b = new object[] { 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "b", ExpectedValue = b, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void Defect_1462300()
        {
            String code =
            @"
            object[] b = new object[] { 1.0, 2.0 };
            ValidationData[] data = { new ValidationData { ValueName = "b", ExpectedValue = b, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryinClass()
        {
            String code =
            @"
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a2", ExpectedValue = 1, BlockIndex = 0 }, new ValidationData { ValueName = "c2", ExpectedValue = c, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryArrayAssignment()
        {
            String code =
            @"
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            object[] d = new object[] { 4.0, 4.0, 4.0 };
            object[] e = new object[] { 3.0, 3.0, 3.0 };
            object[] f = new object[] { 6.0, 6.0, 6.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a11", ExpectedValue = c, BlockIndex = 0 },
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryForLoop()
        {
            String code =
            @"
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            object[] e = new object[] { 3.0, 3.0, 3.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a11", ExpectedValue = c, BlockIndex = 0 },
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryFunction()
        {
            String code =
            @"
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a11", ExpectedValue = c, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryClassInheritance()
        {
            String code =
            @"
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            object[] d = new object[] { 2.0, 2.0, 2.0 };
            ValidationData[] data = { new ValidationData { ValueName = "b2", ExpectedValue = c, BlockIndex = 0 } ,
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryIfElse()
        {
            String code =
            @"
            object[] d = new object[] { 2.0, 2.0, 2.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a1", ExpectedValue = 1, BlockIndex = 0 } ,
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryInlineConditional()
        {
            String code =
            @"
            object[] c = new object[] { 1.0, 1.0, 1.0 };
            object[] d = new object[] { 2.0, 2.0, 2.0 };
            ValidationData[] data = { new ValidationData { ValueName = "s11", ExpectedValue = c, BlockIndex = 0 } ,
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void geometryRangeExpression()
        {
            String code =
            @"
            object[] c = new object[] { 0.5, 0.0, 0.0 };
            ValidationData[] data = { new ValidationData { ValueName = "a12", ExpectedValue = c, BlockIndex = 0 } 
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestExplicitGetterAndSetters()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.DummyBase");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)15, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestNullableTypes()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.DummyBase");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "v111", ExpectedValue = (Int64)111, BlockIndex = 0 },
            Assert.IsTrue(ExecuteAndVerify(code, data) == 0); //runs without any error
        }
        /*  
[Test]

        [Test]
        public void geometryWhileLoop()
        {
            String code =
            @"
            object[] a = new object[] { 6.0, 1.0, 1.0 };
            ValidationData[] data = { new ValidationData { ValueName = "p11", ExpectedValue = a, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void properties()
        {
            String code =
            @"
            double a = 10.000000;
            ValidationData[] data = { new ValidationData { ValueName = "a", ExpectedValue = a, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("Replication")]
        public void coercion_notimplmented()
        {
            String code =
            @"
            object[] c = new object[] { new object[] { 1.0 }, new object[] { null } };
            ValidationData[] data = { new ValidationData { ValueName = "prop", ExpectedValue = c, BlockIndex = 0 } 
            Assert.DoesNotThrow(() => ExecuteAndVerify(code, data), "1467114 Sprint24 : rev 2806 : Replication + function resolution issue : Requested coercion not implemented error message coming when collection has null");
        }

        [Test]
        [Category("Replication")]
        public void coercion_notimplmented2()
        {
            String code =
            @"
            object[] c = new object[] { null };
            ValidationData[] data = { new ValidationData { ValueName = "prop", ExpectedValue = c, BlockIndex = 0 } 
            Assert.DoesNotThrow(() => ExecuteAndVerify(code, data), "1467114 Sprint24 : rev 2806 : Replication + function resolution issue : Requested coercion not implemented error message coming when collection has null");
        }

        [Test]
        [Category("Update")]
        public void SimplePropertyUpdate()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.DummyBase");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)15, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void SimplePropertyUpdateUsingSetMethod()
        {
            string code = @"
            Type dummy = Type.GetType("ProtoFFITests.DummyBase");
            code = string.Format("import(\"{0}\");\r\n{1}", dummy.AssemblyQualifiedName, code);
            ValidationData[] data = { new ValidationData { ValueName = "val", ExpectedValue = (Int64)15, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("Update")]
        public void PropertyReadback()
        {
            string code =
                @"import(""FFITarget.dll"");
            ValidationData[] data = { new ValidationData { ValueName = "readback", ExpectedValue = (Int64)3, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        [Category("Update")]
        public void PropertyUpdate()
        {
            string code =
                @"import(""FFITarget.dll"");
                cls.IntVal = 4;";
            ValidationData[] data = { new ValidationData { ValueName = "readback", ExpectedValue = (Int64)4, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest001()
        {
            string code = @"
            code = string.Format("{0}\r\n{1}\r\n{2}", "import(DisposeVerify from \"ProtoTest.dll\");",
                "import(A from \"ProtoTest.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "m", ExpectedValue = 1, BlockIndex = 0 }, 
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest004()
        {
            string code = @"
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"ProtoTest.dll\");",
                "import(A from \"ProtoTest.dll\");", "import(B from \"ProtoTest.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "m", ExpectedValue = 1, BlockIndex = 0 },  
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest005()
        {
            string code = @"
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"ProtoTest.dll\");",
                "import(A from \"ProtoTest.dll\");", "import(B from \"ProtoTest.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "a", ExpectedValue = 3, BlockIndex = 0 },  
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest002()
        {
            string code = @"
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"ProtoTest.dll\");",
                "import(A from \"ProtoTest.dll\");", "import(B from \"ProtoTest.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "b", ExpectedValue = 1, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest003()
        {
            string code = @"
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"ProtoTest.dll\");",
                "import(A from \"ProtoTest.dll\");", "import(B from \"ProtoTest.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "c", ExpectedValue = 2, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest006()
        {
            // SSA'd transforms will not GC the temps until end of block
            // However, they must be GC's after every line when in debug step over
            // Here 'dv' will not be GC'd until end of block
            string code = @"
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"ProtoTest.dll\");",
                "import(A from \"ProtoTest.dll\");", "import(B from \"ProtoTest.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "v", ExpectedValue = 2, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest007()
        {
            string code = @"
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"ProtoTest.dll\");",
                "import(A from \"ProtoTest.dll\");", "import(B from \"ProtoTest.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "v1", ExpectedValue = 3, BlockIndex = 0 }, 
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void DisposeOnFFITest008()
        {
            string code = @"
            code = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", "import(DisposeVerify from \"ProtoTest.dll\");",
                "import(A from \"ProtoTest.dll\");", "import(B from \"ProtoTest.dll\");", code);
            ValidationData[] data = { new ValidationData { ValueName = "v1", ExpectedValue = 3, BlockIndex = 0 }, 
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestReplicationAndDispose()
        {
            //Defect: DG-1464910 Sprint 22: Rev:2385-324564: Planes get disposed after querying properties on returned planes.
            string code = @"
            code = string.Format("{0}\r\n{1}\r\n{2}", "import(DisposeVerify from \"ProtoTest.dll\");",
                "import(A from \"ProtoTest.dll\");", code);
            object[] b = new object[] { 1, 2, 3 };
            ValidationData[] data = { new ValidationData { ValueName = "c", ExpectedValue = 2, BlockIndex = 0 },
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestNonBrowsableClass()
        {
            string code = @"
            TestFrameWork theTest = new TestFrameWork();
            ExecutionMirror mirror = theTest.RunScriptSource(code);
            Assert.IsTrue(theTest.GetClassIndex("Geometry") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("Point") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("DesignScriptEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("GeometryFactory") == ProtoCore.DSASM.Constants.kInvalidIndex);
        }

        [Test]
        public void TestImportNonBrowsableClass()
        {
            string code = @"
            TestFrameWork theTest = new TestFrameWork();
            ExecutionMirror mirror = theTest.RunScriptSource(code);
            Assert.IsTrue(theTest.GetClassIndex("Geometry") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("Point") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("DesignScriptEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("GeometryFactory") == ProtoCore.DSASM.Constants.kInvalidIndex);
        }

        [Test]
        public void TestImportBrowsableClass()
        {
            string code = @"
            TestFrameWork theTest = new TestFrameWork();
            ExecutionMirror mirror = theTest.RunScriptSource(code);
            //This import must import BSplineCurve and related classes.
            Assert.IsTrue(theTest.GetClassIndex("Geometry") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("Point") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("Vector") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("Solid") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("Surface") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("Plane") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("CoordinateSystem") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("CoordinateSystem") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("Curve") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("NurbsCurve") != ProtoCore.DSASM.Constants.kInvalidIndex);
            //Non-browsable as well as unrelated class should not be imported.
            Assert.IsTrue(theTest.GetClassIndex("DesignScriptEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("Circle") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("SubDivisionMesh") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("GeometryFactory") == ProtoCore.DSASM.Constants.kInvalidIndex);
        }

        [Test]
        public void TestNonBrowsableInterfaces()
        {
            string code = @"
            TestFrameWork theTest = new TestFrameWork();
            ExecutionMirror mirror = theTest.RunScriptSource(code);
            Assert.IsTrue(theTest.GetClassIndex("Geometry") != ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IColor") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IDesignScriptEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IDisplayable") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IPersistentObject") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IPersistencyManager") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("ICoordinateSystemEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IGeometryEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IPointEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("ICurveEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("ILineEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("ICircleEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IArcEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IBSplineCurveEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IBRepEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("ISurfaceEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IBSplineSurfaceEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IPlaneEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("ISolidEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IPrimitiveSolidEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IConeEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("ICuboidEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("ISphereEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IPolygonEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("ISubDMeshEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IBlockEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IBlockHelper") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("ITopologyEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IShellEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("ICellEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IFaceEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("ICellFaceEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IVertexEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IEdgeEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("ITextEntity") == ProtoCore.DSASM.Constants.kInvalidIndex);
            Assert.IsTrue(theTest.GetClassIndex("IGeometryFactory") == ProtoCore.DSASM.Constants.kInvalidIndex);
        }

        [Test]
        public void TestDefaultConstructorNotAvailableOnAbstractClass()
        {
            string code = @"
            TestFrameWork theTest = new TestFrameWork();
            ExecutionMirror mirror = theTest.RunScriptSource(code);
            //Verify that Geometry.Geometry constructor deson't exists
            theTest.VerifyMethodExists("Geometry", "Geometry", false);
        }

        [Test]
        public void T023_Abstract_FFI_1467159()
        {
            //Defect: DG-1464910 Sprint 22: Rev:2385-324564: Planes get disposed after querying properties on returned planes.
            string code = @"
            code = string.Format("{0}\r\n{1}\r\n{2}", "import(DisposeVerify from \"ProtoTest.dll\");",
                "import(A from \"ProtoTest.dll\");", code);
            thisTest.RunScriptSource(code);
            thisTest.Verify("g", null);
            thisTest.Verify("a", null);
            // thisTest.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);
        }


        [Test]
        public void T024_disposeininclude_1467252()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string testPath = "..\\..\\..\\Scripts\\ffitests\\";
            /*string code = @"
            string error = "1467252 - Declaring a geometry in 2 files and running with including one into another ";
            string code = @"
 import(""ProtoGeometry.dll"");
 import(""file_1467252.ds"");
 WCS = CoordinateSystem.Identity();
 newCS = WCS.Translate(Vector.ByCoordinates(0, 1, 0));";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage(error);
            thisTest.VerifyRuntimeWarningCount(1);
            //});
        }

        [Test]
        public void T025_disposeinimperative_1464937()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string testPath = "..\\..\\..\\Scripts\\ffitests\\";
            /*string code = @"
            string error = "";
            string code = @"
class point{
    a;
constructor point(x:double)
{
a = 1;
}
}
class polygon{
constructor polygon(pt:point[])
{
poly= pt;
}
}
pt0=point.point(1.0);
pt1=point.point(2.0);
pt2=point.point(3.0);
pt3=point.point(4.0);
pointGroup = {pt0,pt1};
z=[Imperative]
{
def buildarray(test:int[],collect:point[])
{
b= { } ;
j=0;
for (k in test)
{
b[j] = collect[k];
j=j+1;
}
return =b;
}
controlPoly={};
c={0,1};
a=buildarray(c,pointGroup);
c={pt0,pt1};
controlPoly = polygon.polygon(a);
controlPoly2 = polygon.polygon(c);
return=a;
}
z1 = z.a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage(error);
            thisTest.Verify("z1", new object[] { 1, 1 });

            //});
        }

        [Test]
        public void TestColorComparison()
        {
            string code =
                @"import(""ProtoGeometry.dll"");
            ValidationData[] data = { new ValidationData { ValueName = "failure", ExpectedValue = false, BlockIndex = 0 },
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestNestedClass()
        {
            string code =
               @"import(NestedClass from ""ProtoTest.dll"");
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestNestedClassImport()
        {
            string code =
               @"import(NestedClass from ""ProtoTest.dll"");
            ValidationData[] data = { new ValidationData { ValueName = "success", ExpectedValue = true, BlockIndex = 0 } };
            ExecuteAndVerify(code, data);
        }

        [Test]
        public void TestNamespaceImport()
        {
            string code =
                @"import(MicroFeatureTests from ""ProtoTest.dll"");";
            TestFrameWork theTest = new TestFrameWork();
            var mirror = theTest.RunScriptSource(code);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kMultipleSymbolFound);
            string[] classes = theTest.GetAllMatchingClasses("MicroFeatureTests");
            Assert.True(classes.Length > 1, "More than one implementation of MicroFeatureTests class expected");
        }

        [Test]
        public void TestNamespaceClassResolution()
        {
            string code =
                @"import(""FFITarget.dll"");
                    x = 1..2;
                    aDup = A.DupTargetTest(x);
                    bDup = B.DupTargetTest(x);

                    check = Equals(aDup.Prop,bDup.Prop);";

            TestFrameWork theTest = new TestFrameWork();
            var mirror = theTest.RunScriptSource(code);
            theTest.Verify("check", true);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kMultipleSymbolFound);
            string[] classes = theTest.GetAllMatchingClasses("DupTargetTest");
            Assert.True(classes.Length > 1, "More than one implementation of DupTargetTest class expected");
        }

        [Test]
        public void TestSubNamespaceClassResolution()
        {
            string code =
                @"import(""FFITarget.dll"");
                    aDup = A.DupTargetTest(0);
                    bDup = B.DupTargetTest(1); //This should match exactly B.DupTargetTest
                    cDup = C.B.DupTargetTest(2);

                    check = Equals(aDup.Prop,bDup.Prop);
                    check = Equals(bDup.Prop,cDup.Prop);

";

            TestFrameWork theTest = new TestFrameWork();
            var mirror = theTest.RunScriptSource(code);
            theTest.Verify("check", true);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kMultipleSymbolFound);
            string[] classes = theTest.GetAllMatchingClasses("DupTargetTest");
            Assert.True(classes.Length > 1, "More than one implementation of DupTargetTest class expected");
        }
    }

    // the following classes are used to test Dispose method call on FFI
    public class DisposeVerify
    {
        public static int val = 10;
        public static DisposeVerify CreateObject()
        {
            return new DisposeVerify();
        }
        public static int GetValue()
        {
            return val;
        }
        public static int SetValue(int _val)
        {
            val = _val;
            return val;
        }
    }
    public class A : IDisposable
    {
        private int val;
        public static A CreateObject(int _val)
        {
            return new A { val = _val };
        }
        public int Value
        {
            get { return val; }
        }
        public void Dispose()
        {
            DisposeVerify.SetValue(val);
        }
    }
    public class B : IDisposable
    {
        private int val;
        public static B CreateObject(int _val)
        {
            return new B { val = _val };
        }
        public void Dispose()
        {
            DisposeVerify.val += val;
        }
    }
    public class NestedClass
    {
        public static Type GetType(int value)
        {
            return new Type(value);
        }
        public static bool CheckType(Type t, int value)
        {
            return t._x == value;
        }
        public class Type
        {
            public int _x;
            public Type(int x)
            {
                _x = x;
            }
            public bool Equals(Type obj)
            {
                return obj._x == this._x;
            }
        }
    }

    namespace DesignScript
    {
        public class Point
        {
            public static Point XYZ(double x, double y, double z)
            {
                var p = new Point { dX = x, dY = y, dZ = z };
                return p;
            }

            public double dX { get; set; }
            public double dY { get; set; }
            public double dZ { get; set; }
        }
    }

    namespace Dynamo
    {
        public class Point
        {
            public static Point XYZ(double x, double y, double z)
            {
                var p = new Point { X = x, Y = y, Z = z };
                return p;
            }

            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
        }
    }
}