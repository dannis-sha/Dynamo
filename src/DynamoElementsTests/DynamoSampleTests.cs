﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dynamo.Controls;
using Dynamo.Nodes;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class DynamoSampleTests
    {

        #region startup and shutdown

        [SetUp]
        public void Init()
        {
            StartDynamo();
        }

        [TearDown]
        public void Cleanup()
        {
            try
            {
                DynamoLogger.Instance.FinishLogging();
                controller.ShutDown();

                EmptyTempFolder();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static DynamoController controller;
        private static string TempFolder;
        private static string ExecutingDirectory { get; set; }

        private static void StartDynamo()
        {
            try
            {
                ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string tempPath = Path.GetTempPath();

                TempFolder = Path.Combine(tempPath, "dynamoTmp");

                if (!Directory.Exists(TempFolder))
                {
                    Directory.CreateDirectory(TempFolder);
                }
                else
                {
                    EmptyTempFolder();
                }

                DynamoLogger.Instance.StartLogging();

                //create a new instance of the ViewModel
                controller = new DynamoController(new FSchemeInterop.ExecutionEnvironment(), false, typeof(DynamoViewModel), Context.NONE);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void EmptyTempFolder()
        {
            try
            {
                var directory = new DirectoryInfo(TempFolder);
                foreach (FileInfo file in directory.GetFiles()) file.Delete();
                foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        #endregion

        #region utility methods

        public dynNodeModel NodeFromCurrentSpace(DynamoViewModel vm, string guidString)
        {
            Guid guid = Guid.Empty;
            Guid.TryParse(guidString, out guid);
            return NodeFromCurrentSpace(vm, guid);
        }
 
        public dynNodeModel NodeFromCurrentSpace(DynamoViewModel vm, Guid guid)
        {
            return vm.CurrentSpace.Nodes.FirstOrDefault((node) => node.GUID == guid);
        }

        public dynWatch GetWatchNodeFromCurrentSpace(DynamoViewModel vm, string guidString)
        {
            var nodeToWatch = NodeFromCurrentSpace(vm, guidString);
            Assert.NotNull(nodeToWatch);
            Assert.IsAssignableFrom(typeof(dynWatch), nodeToWatch);
            return (dynWatch)nodeToWatch;
        }

        public double GetDoubleFromFSchemeValue(FScheme.Value value)
        {
            var doubleWatchVal = 0.0;
            Assert.AreEqual(true, FSchemeInterop.Utils.Convert(value, ref doubleWatchVal));
            return doubleWatchVal;
        }

        public FSharpList<FScheme.Value> GetListFromFSchemeValue(FScheme.Value value)
        {
            FSharpList<FScheme.Value> listWatchVal = null;
            Assert.AreEqual(true, FSchemeInterop.Utils.Convert(value, ref listWatchVal));
            return listWatchVal;
        }

        #endregion

        [Test]
        public void AddSubtractMapReduceFilterBasic()
        {
            var vm = controller.DynamoViewModel;

            string openPath = Path.Combine(ExecutingDirectory, @"..\..\test\dynamo_elements_samples\working\map_reduce_filter\map_reduce_filter.dyn");
            controller.RunCommand( vm.OpenCommand, openPath );

            // check all the nodes and connectors are loaded
            Assert.AreEqual(28, vm.CurrentSpace.Connectors.Count);
            Assert.AreEqual(28, vm.CurrentSpace.Nodes.Count);

            // check an input value
            var node1 = NodeFromCurrentSpace(vm, "51ed7fed-99fa-46c3-a03c-2c076f2d0538");
            Assert.NotNull(node1);
            Assert.IsAssignableFrom(typeof(dynDoubleInput), node1);
            Assert.AreEqual(2.0, ((dynDoubleInput)node1).Value);
            
            // run the expression
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed

            // add-subtract -3.0
            var watch = GetWatchNodeFromCurrentSpace(vm, "4a2363b6-ef64-44f5-be64-18832586e574");
            var doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(-3.0, doubleWatchVal);

            // map - list of three 6's 
            watch = GetWatchNodeFromCurrentSpace(vm,  "fcad8d7a-1c9f-4604-a03b-53393e36ea0b");
            FSharpList<FScheme.Value> listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(3, listWatchVal.Length);
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal[0]));
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal[1]));
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal[2]));

            // reduce - 6.0
            watch = GetWatchNodeFromCurrentSpace(vm, "e892c469-47e6-4006-baea-ec4afea5a04e");
            doubleWatchVal = GetDoubleFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(6.0, doubleWatchVal);

            // filter - list of 6-10
            watch = GetWatchNodeFromCurrentSpace(vm, "41279a88-2f0b-4bd3-bef1-1be693df5c7e");
            listWatchVal = GetListFromFSchemeValue(watch.GetValue(0));
            Assert.AreEqual(5, listWatchVal.Length);
            Assert.AreEqual(6, GetDoubleFromFSchemeValue(listWatchVal[0]));
            Assert.AreEqual(7, GetDoubleFromFSchemeValue(listWatchVal[1]));
            Assert.AreEqual(8, GetDoubleFromFSchemeValue(listWatchVal[2]));
            Assert.AreEqual(9, GetDoubleFromFSchemeValue(listWatchVal[3]));
            Assert.AreEqual(10, GetDoubleFromFSchemeValue(listWatchVal[4]));

        }

        [Test]
        public void Sequence()
        {
            var vm = controller.DynamoViewModel;

            string openPath = Path.Combine(ExecutingDirectory, @"..\..\test\dynamo_elements_samples\working\sequence\sequence.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, vm.CurrentSpace.Connectors.Count);
            Assert.AreEqual(8, vm.CurrentSpace.Nodes.Count);

            // run the expression
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed

        }

        [Test]
        public void CombineWithCustomNodes()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\dynamo_elements_samples\working\combine\");

            Assert.IsTrue( controller.CustomNodeLoader.AddFileToPath(Path.Combine(examplePath, "combine2.dyf")));
            Assert.IsTrue( controller.CustomNodeLoader.AddFileToPath(Path.Combine(examplePath, "Sequence2.dyf")));

            string openPath = Path.Combine(examplePath, "combine-with-three.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, vm.CurrentSpace.Connectors.Count);
            Assert.AreEqual(10, vm.CurrentSpace.Nodes.Count);

            // run the expression
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed


        }

        [Test]
        public void ReduceAndRecursion()
        {
            var vm = controller.DynamoViewModel;

            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\dynamo_elements_samples\working\reduce_and_recursion\");

            Assert.IsTrue(controller.CustomNodeLoader.AddFileToPath(Path.Combine(examplePath, "MyReduce.dyf")));
            Assert.IsTrue(controller.CustomNodeLoader.AddFileToPath(Path.Combine(examplePath, "Sum Numbers.dyf")));

            string openPath = Path.Combine(examplePath, "reduce-example.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, vm.CurrentSpace.Connectors.Count);
            Assert.AreEqual(11, vm.CurrentSpace.Nodes.Count);

            // run the expression
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed


        }

        [Test]
        public void FilterWithCustomNode()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\dynamo_elements_samples\working\filter\");

            Assert.IsTrue(controller.CustomNodeLoader.AddFileToPath(Path.Combine(examplePath, "IsOdd.dyf")));

            string openPath = Path.Combine(examplePath, "filter-example.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, vm.CurrentSpace.Connectors.Count);
            Assert.AreEqual(6, vm.CurrentSpace.Nodes.Count);

            // run the expression
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed


        }

        [Test]
        public void Sorting()
        {
            var vm = controller.DynamoViewModel;
            var examplePath = Path.Combine(ExecutingDirectory, @"..\..\test\dynamo_elements_samples\working\sorting\");

            string openPath = Path.Combine(examplePath, "sorting.dyn");
            controller.RunCommand(vm.OpenCommand, openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, vm.CurrentSpace.Connectors.Count);
            Assert.AreEqual(11, vm.CurrentSpace.Nodes.Count);

            // run the expression
            controller.RunCommand(vm.RunExpressionCommand);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed


        }

    }
}