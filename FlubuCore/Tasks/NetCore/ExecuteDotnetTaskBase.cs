﻿using System.Collections.Generic;
using FlubuCore.Context;
using FlubuCore.Tasks.Process;

namespace FlubuCore.Tasks.NetCore
{
    public abstract class ExecuteDotnetTaskBase<TTask> : TaskBase<int, TTask>
        where TTask : class, ITask
    {
        private string _workingFolder;
        private string _dotnetExecutable;
        private bool _doNotLogOutput;

        public ExecuteDotnetTaskBase(string command)
        {
            Command = command;
        }

        public ExecuteDotnetTaskBase(StandardDotnetCommands command)
        {
            Command = command.ToString().ToLowerInvariant();
        }

        internal List<string> Arguments { get; } = new List<string>();

        /// <summary>
        /// Dotnet command to be executed.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Add's Argument to the dotnet see <c>Command</c>
        /// </summary>
        /// <param name="arg">Argument to be added</param>
        /// <returns></returns>
        public TTask WithArguments(string arg)
        {
            Arguments.Add(arg);
            return this as TTask;
        }

        /// <summary>
        /// Add's Arguments to the dotnet see <c>Command</c>
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public TTask WithArguments(params string[] args)
        {
            Arguments.AddRange(args);
            return this as TTask;
        }

        /// <summary>
        /// Working folder of the dotnet command
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public TTask WorkingFolder(string folder)
        {
            _workingFolder = folder;
            return this as TTask;
        }

        /// <summary>
        /// Path to the dotnet executable.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public TTask DotnetExecutable(string fullPath)
        {
            _dotnetExecutable = fullPath;
            return this as TTask;
        }

        public TTask DoNotLogOutput()
        {
            _doNotLogOutput = true;
            return this as TTask;
        }

        protected override int DoExecute(ITaskContextInternal context)
        {
            string program = _dotnetExecutable;

            if (string.IsNullOrEmpty(program))
            {
                program = context.Properties.GetDotnetExecutable();
            }

            if (string.IsNullOrEmpty(program))
            {
                context.Fail("Dotnet executable not set!", -1);
                return -1;
            }

            BeforeExecute(context);
            IRunProgramTask task = context.Tasks().RunProgramTask(program);

            if (_doNotLogOutput)
                task.DoNotLogOutput();

            if (DoNotLog)
                task.NoLog();

            task
                .WithArguments(Command)
                .WithArguments(Arguments.ToArray())
                .WorkingFolder(_workingFolder)
                .CaptureErrorOutput()
                .CaptureOutput()
                .ExecuteVoid(context);

            return 0;
        }

        protected virtual void BeforeExecute(ITaskContextInternal context)
        {
        }
    }
}