//
// Author:
//   Michael Göricke
//
// Copyright (c) 2019
//
// This file is part of ShapeConverter.
//
// ShapeConverter is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see<http://www.gnu.org/licenses/>.

#if DEBUG
#define DUMP1
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using EpsSharp.Eps.Content;
using EpsSharp.Eps.Commands;
using ShapeConverter;

namespace EpsSharp.Eps.Core
{
    /// <summary>
    /// The EPS interpreter engine
    /// </summary>
    internal class EpsInterpreter
    {
        private Parser parser;
        private EpsDictionary statusDict;
        private EpsDictionary fontDict;
        private int procedureCreationLevel;
        private int stoppedContextLevel;
        Stack<GraphicsState> graphicsStateStack;
        private bool quitInterpreter;
        private bool exitCurrentLoop;
        private bool stopProcedure;

        public EpsInterpreter()
        {
            OperandStack = new EpsStack<Operand>();
        }

        /// <summary>
        /// The operand stack
        /// </summary>
        public EpsStack<Operand> OperandStack { get; private set; }

        /// <summary>
        /// The dictionary stack
        /// </summary>
        public EpsStack<EpsDictionary> DictionaryStack { get; private set; }

        /// <summary>
        /// The system dictionary
        /// </summary>
        public EpsDictionary SystemDict { get; private set; }

        /// <summary>
        /// The global dictionary
        /// </summary>
        public EpsDictionary GlobalDict { get; private set; }

        /// <summary>
        /// The user dictionary
        /// </summary>
        public EpsDictionary UserDict { get; private set; }

        /// <summary>
        /// The error dictionary
        /// </summary>
        public EpsDictionary ErrorDict { get; private set; }

        /// <summary>
        /// The $error dictionary
        /// </summary>
        public EpsDictionary DollarErrordict { get; private set; }

        /// <summary>
        /// The current dictionary
        /// </summary>
        public EpsDictionary CurrentDict => DictionaryStack.Peek();

        /// <summary>
        /// the current graphic state
        /// </summary>
        public GraphicsState GraphicState { get; set; }

        /// <summary>
        /// The result of the interpreter
        /// </summary>
        public GraphicGroup ReturnGraphicGroup { get; set; }

        /// <summary>
        /// The current graphic group
        /// </summary>
        public GraphicGroup GraphicGroup { get; set; }

        /// <summary>
        /// The stream the interpreter reads the script from
        /// </summary>
        public EpsStreamReader FileReader { get; private set; }

        /// <summary>
        /// The resource manager
        /// </summary>
        public ResourceManager ResourceManager { get; private set; }

        /// <summary>
        /// The procedure creation level
        /// 0 means: in standard operation mode
        /// > 0 means: in procedure creation mode, the value indicates the nesting level
        /// </summary>
        public int ProcedureCreationLevel => procedureCreationLevel;

        /// <summary>
        /// The stopped context level
        /// 0 means: the interpreter is in no stopped context
        /// > 0 means: the interpreter is in a stopped context, the value indicates the nesting level
        /// </summary>
        public bool IsInStoppedContext => stoppedContextLevel > 0;

        /// <summary>
        /// Gets a value indicating whether a running loop is to be broken
        /// (stopped at the current point of execution)
        /// </summary>
        public bool BreakCurrentLoop => exitCurrentLoop || stopProcedure || quitInterpreter;

        /// <summary>
        /// The bounding box of the document
        /// </summary>
        public Rect BoundingBox { get; set; }

        /// <summary>
        /// Set the exit current loop indicator
        /// </summary>
        public void ExitCurrentLoop()
        {
            exitCurrentLoop = true;
        }

        /// <summary>
        /// Reset the exit current loop indicator
        /// </summary>
        public void ResetExitCurrentLoop()
        {
            exitCurrentLoop = false;
        }

        /// <summary>
        /// Set the stop procedure indicator to "stop"
        /// hint: stop also breaks a running loop
        /// </summary>
        public void StopProcedure()
        {
            stopProcedure = true;
        }

        /// <summary>
        /// Return the state of the stop procedure indicator
        /// </summary>
        /// <returns></returns>
        public bool GetStopProcedureStatus()
        {
            return stopProcedure;
        }

        /// <summary>
        /// Quit the execution of the interpreter
        /// </summary>
        public void QuitExecution()
        {
            quitInterpreter = true;
        }

        /// <summary>
        /// Enter a procedure creation level
        /// </summary>
        public void EnterProcedureCreation()
        {
            procedureCreationLevel++;
        }

        /// <summary>
        /// Leave a procedure creation level
        /// </summary>
        public void LeaveProcedureCreation()
        {
            procedureCreationLevel--;
        }

        /// <summary>
        /// Enter a stopped context level
        /// </summary>
        public void EnterStoppedContext()
        {
            stopProcedure = false;
            stoppedContextLevel++;
        }

        /// <summary>
        /// Leave a stopped context level
        /// </summary>
        public void LeaveStoppedContext()
        {
            stopProcedure = false;
            stoppedContextLevel--;
        }

        /// <summary>
        /// Reset the current path
        /// </summary>
        public void ResetCurrentGeometry()
        {
            GraphicState.CurrentGeometry = null;
        }

        /// <summary>
        /// Push the current graphic state onto the stack
        /// </summary>
        public void PushGraphicsState()
        {
            var clone = GraphicState.Clone();
            graphicsStateStack.Push(clone);
        }

        /// <summary>
        /// Pop a graphic state from the stack
        /// </summary>
        public void PopGraphicsState()
        {
            GraphicState = graphicsStateStack.Pop();
        }

        /// <summary>
        /// Run the interpreter
        /// </summary>
        public GraphicGroup Run(StreamReader scriptStream, int postscriptVersion)
        {
            PrepareStandardDictionaries(postscriptVersion);
            AllCommands.Register(this);
            InitGraphic();

            FileReader = new EpsStreamReader(scriptStream);
            parser = new Parser(FileReader);

            Run();

            return ReturnGraphicGroup;
        }

        /// <summary>
        /// Execute a single operand
        /// </summary>
        /// <param name="op"></param>
        public void Execute(Operand op)
        {
            ExecuteInternal(op);
        }

        /// <summary>
        /// Main loop
        /// </summary>
        private void Run()
        {
            Operand op = parser.GetNextOperand();

            while (op != null)
            {
                ExecuteInternal(op);

                if (quitInterpreter)
                {
                    break;
                }

                op = parser.GetNextOperand();
            }
        }

        /// <summary>
        /// Process a single operand
        /// </summary>
        /// <param name="op"></param>
        private void ExecuteInternal(Operand op)
        {
            if (op.IsExecutable && (procedureCreationLevel == 0 || op.AlwaysExecute))
            {
#if DUMP
                TraceAll(op, "exec");
#endif

                op.Execute(this);
            }
            else
            {
#if DUMP
                TraceAll(op, "push");
#endif

                OperandStack.Push(op.Copy());
            }
        }

#if DEBUG
        /// <summary>
        /// Dump the operand stack
        /// </summary>
        public void DumpOperandStack()
        {
            Console.Write("{0,6}  stack  ",  " ");
            int index = 0;

            for (int i = OperandStack.Count - 1; i >= 0; i--)
            {
                Console.Write(string.Format("[{0}]{1}  ", index, OperandStack[i].ToString()));
                index++;
            }

            Console.WriteLine();
        }
#endif

#if DUMP
        private void TraceAll(Operand op, string operation)
        {
            if (op.LineNumber != 0 && procedureCreationLevel == 0)
            {
                //DumpSimpleDictionaryStack();
                DumpOperandStack();
                DumpOperation(op, operation);
            }
        }

        /// <summary>
        /// Dump a single operand plus its operation
        /// </summary>
        private void DumpOperation(Operand op, string operation)
        {
            Console.WriteLine(string.Format("{0,6}  {1}   {2}", op.LineNumber, operation, op.ToString()));
        }

        /// <summary>
        /// Dump the contents of all dictionaries of the dictionary stack
        /// </summary>
        public void DumpSimpleDictionaryStack()
        {
            Console.Write("{0,6}  dict   ", " ");
            int index = 0;

            for (int i = DictionaryStack.Count - 1; i >= 0; i--)
            {
                var dict = DictionaryStack[i];
                Console.Write($"[{index}]{dict.Count}  ");
                index++;
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Dump the contents of all dictionaries of the dictionary stack
        /// </summary>
        public void DumpFullDictionaryStack()
        {
            foreach (var dict in DictionaryStack)
            {
                foreach (var e in dict)
                {
                    if (e.Key is NameOperand n)
                    {
                        Console.WriteLine(n.Value);
                    }
                }

                Console.WriteLine("------------------------------------------------------");
            }
        }
#endif

        /// <summary>
        /// Initialize all about graphic
        /// </summary>
        private void InitGraphic()
        {
            ReturnGraphicGroup = new GraphicGroup();
            GraphicGroup = ReturnGraphicGroup;

            graphicsStateStack = new Stack<GraphicsState>();

            GraphicState = new GraphicsState();
            GraphicState.TransformationMatrix = Matrix.Identity;
            GraphicState.ColorSpace = ColorSpaceActivator.CreateColorSpace(this, EpsKeys.DeviceGray);

            var values = new List<Operand> { new RealOperand(0) };  // black

            GraphicState.FillBrush = GraphicState.ColorSpace.GetBrushDescriptor(values,
                                                                   GraphicState.CurrentTransformationMatrix);
        }

        /// <summary>
        /// Prepare dictionaries
        /// </summary>
        private void PrepareStandardDictionaries(int postscriptVersion)
        {
            ResourceManager = new ResourceManager();
            ResourceManager.DefineResource("Category", new NameOperand("Generic"), new DictionaryOperand());

            DictionaryStack = new EpsStack<EpsDictionary>();
            SystemDict = new EpsDictionary { IsPermanent = true };
            GlobalDict = new EpsDictionary { IsPermanent = true };
            UserDict = new EpsDictionary { IsPermanent = true };
            ErrorDict = new EpsDictionary { IsPermanent = true };
            DollarErrordict = new EpsDictionary { IsPermanent = true };
            statusDict = new EpsDictionary { IsPermanent = true };
            fontDict = new EpsDictionary { IsPermanent = true };

            DictionaryStack.Push(SystemDict);
            DictionaryStack.Push(GlobalDict);
            DictionaryStack.Push(UserDict);

            var systemDict = SystemDict;

            var name = new NameOperand("systemdict");
            Operand operand = new DictionaryOperand(SystemDict);
            systemDict.Add(name, operand);

            name = new NameOperand("userdict");
            operand = new DictionaryOperand(UserDict);
            systemDict.Add(name, operand);

            name = new NameOperand("globaldict");
            operand = new DictionaryOperand(GlobalDict);
            systemDict.Add(name, operand);

            name = new NameOperand("statusdict");
            operand = new DictionaryOperand(statusDict);
            systemDict.Add(name, operand);

            name = new NameOperand("errordict");
            operand = new DictionaryOperand(ErrorDict);
            systemDict.Add(name, operand);

            name = new NameOperand("$error");
            operand = new DictionaryOperand(DollarErrordict);
            systemDict.Add(name, operand);

            name = new NameOperand("GlobalFontDirectory");
            operand = new DictionaryOperand(fontDict);
            systemDict.Add(name, operand);

            name = new NameOperand("SharedFontDirectory");
            operand = new DictionaryOperand(fontDict);
            systemDict.Add(name, operand);

            name = new NameOperand("true");
            operand = new BooleanOperand(true);
            systemDict.Add(name, operand);

            name = new NameOperand("false");
            operand = new BooleanOperand(false);
            systemDict.Add(name, operand);

            name = new NameOperand("null");
            operand = new NullOperand();
            systemDict.Add(name, operand);

            // for level 1 do not create this name, some adobe scripts
            // assume level 2 or higher just based on the existence of the name 

            if (postscriptVersion >= 2)
            {
                name = new NameOperand("languagelevel");
                operand = new IntegerOperand(postscriptVersion);
                systemDict.Add(name, operand);
            }

            name = new NameOperand("newerror");
            var b = new BooleanOperand(false);
            DollarErrordict.Add(name, b);
        }

        /// <summary>
        /// Register all commands
        /// </summary>
        private void RegisterCommands()
        {
        }

        /// <summary>
        /// Register a single command
        /// </summary>
        /// <param name="cmdName"></param>
        /// <param name="cmd"></param>
        internal void RegisterCommand(string cmdName, CommandOperand cmd)
        {
            var name = new NameOperand();
            name.Value = cmdName;
            cmd.Name = $"build-in:{cmdName}";  // this name is for debugging purposes only
            SystemDict.Add(name, cmd);
        }

        /// <summary>
        /// Register a single command
        /// </summary>
        /// <param name="cmdName"></param>
        /// <param name="cmd"></param>
        internal void RegisterErrorCommand(string cmdName, CommandOperand cmd)
        {
            var name = new NameOperand();
            name.Value = cmdName;
            cmd.Name = $"build-in:{cmdName}";  // this name is for debugging purposes only
            ErrorDict.Add(name, cmd);
        }
    }
}
