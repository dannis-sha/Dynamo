using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Imperative
{
    public class Imperative
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\Imperative\\Imperative\\";
        [SetUp]
        public void Setup()
        {
        }


        [Test]
        [Category("Imperative")]
        public void T002_ClassConstructorNestedScope_InlineCondition()
        {
            String code =
             @"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 1);
        }

        [Test]
        [Category("Imperative")]
        public void T003_ClassConstructorNestedScope_RangeExpr()
        {
            String code =
             @"
            string errmsg = "DNL-1467610 When for loop is called inside nested scope ( imp-assoc-imp ), it throws an unexpected error message";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.VerifyRuntimeWarningCount(0);
            thisTest.Verify("t1", new Object[] { 1, 1, 2, 2, 3 });
        }

        [Test]
        [Category("Imperative")]
        public void T004_ClassConstructorNestedScope_TypeConversion()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyRuntimeWarningCount(1);
            thisTest.Verify("t1", new Object[] { 0, null });
        }

        [Test]
        [Category("Imperative")]
        public void T005_ClassConstructorNestedScope_TypeConversion()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyRuntimeWarningCount(1);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("Imperative")]
        public void T006_ClassConstructorNestedScope_FunctionCall()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 11);
        }

        [Test]
        [Category("Imperative")]
        public void T007_ClassConstructorNestedScope_ArrayCreation()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] x = new Object[] { 2, 3, 6 };
            thisTest.Verify("a1", x);
            thisTest.Verify("a2", x);
            thisTest.Verify("a3", x);
            thisTest.Verify("a4", x);
        }

        [Test]
        [Category("Imperative")]
        public void T008_ClassConstructorNestedScope_ArrayIndexing()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", new Object[] { 0, 1, 3 });
            thisTest.Verify("a2", new Object[] { 0, 1 });

        }

        [Test]
        [Category("Imperative")]
        public void T009_ClassConstructorNestedScope_LogicalOperators()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", false);
            thisTest.Verify("a2", true);
        }

        [Test]
        [Category("Imperative")]
        public void T010_ClassConstructorNestedScope_RelationalOperators()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 0);
            thisTest.Verify("a2", 1);
            thisTest.Verify("a3", 1);
        }

        [Test]
        [Category("Imperative")]
        public void T011_ClassConstructorNestedScope_MathematicalOperators()
        {
            String code =
             @"
            string errmsg = "DNL-1467612 Using the 'mod' operator on double value yields null in imperative scope and an unexpected warning message in associative scope";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 1.0);
        }

        [Test]
        [Category("Imperative")]
        public void T012_ClassConstructorNestedScope_ImplicitTypeConversion()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 3);
            thisTest.Verify("a2", 3);
        }

        [Test]
        [Category("Imperative")]
        public void T013_ClassConstructorNestedScope_UseThisKeyWord()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 3);
            thisTest.Verify("a2", 7);
        }

        [Test]
        [Category("Imperative")]
        public void T014_ClassConstructorNestedScope_CompareClassesUsingThis()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("c", false);
        }

        [Test]
        [Category("Imperative")]
        public void T015_ClassConstructorNestedScope_GlobalVariableInCode()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 1);
        }

        [Test]
        [Category("Imperative")]
        public void T016_ClassConstructorNestedScope_GlobalVariableInArgument()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 0);
        }

        [Test]
        [Category("Imperative")]
        public void T017_ClassConstructorNestedScope_UpdateInSameScope()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 0);
        }

        [Test]
        [Category("Imperative")]
        public void T018_ClassConstructorNestedScope_UpdateInInnerScope()
        {
            String code =
             @"
            string errmsg = "DNL-1461388 Sprint 19 : rev 1808 : Cross Language Update Issue : Inner Associative block should trigger update of outer associative block variable";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 2);
        }

        [Test]
        [Category("Imperative")]
        public void T018_ClassConstructorNestedScope_UpdateInOuterScope()
        {
            String code =
             @"
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 1);

        }
    }
}