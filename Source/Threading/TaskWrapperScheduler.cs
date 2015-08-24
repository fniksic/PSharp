﻿//-----------------------------------------------------------------------
// <copyright file="TaskWrapperScheduler.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// 
//      THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
//      EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
//      OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
//      The example companies, organizations, products, domain names,
//      e-mail addresses, logos, people, places, and events depicted
//      herein are fictitious.  No association with any real company,
//      organization, product, domain name, email address, logo, person,
//      places, or events is intended or should be inferred.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.PSharp.Tooling;

namespace Microsoft.PSharp.Threading
{
    /// <summary>
    /// Class implementing the P# task wrapper scheduler.
    /// </summary>
    internal sealed class TaskWrapperScheduler : TaskScheduler
    {
        #region fields

        /// <summary>
        /// The machine tasks.
        /// </summary>
        private List<Task> MachineTasks;

        /// <summary>
        /// The wrapped in a machine user tasks.
        /// </summary>
        private List<Task> WrappedTasks;

        #endregion

        #region internal methods

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="machineTasks">Machine tasks</param>
        internal TaskWrapperScheduler(List<Task> machineTasks)
        {
            this.MachineTasks = machineTasks;
            this.WrappedTasks = new List<Task>();
        }

        /// <summary>
        /// Executes the given scheduled task.
        /// </summary>
        /// <param name="task">Task</param>
        internal void Execute(Task task)
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                base.TryExecuteTask(task);
            }, null);
        }

        #endregion

        #region override methods

        /// <summary>
        /// Enqueues the given task. If the task does not correspond to a P# machine,
        /// then it wraps it in a task machine and schedules it.
        /// </summary>
        /// <param name="task">Task</param>
        protected override void QueueTask(Task task)
        {
            if (this.MachineTasks.Contains(task))
            {
                this.Execute(task);
            }
            else
            {
                Output.Debug(DebugType.Testing, "<ScheduleDebug> Wrapping task {0} in a machine.", task.Id);
                this.WrappedTasks.Add(task);
                Machine.Dispatcher.TryCreateTaskMachine(task);
            }
        }

        /// <summary>
        /// Tries to execute the task inline.
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="taskWasPreviouslyQueued">Boolean value</param>
        /// <returns>Boolean value</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        /// <summary>
        /// Returns the wrapped in a machine scheduled tasks.
        /// </summary>
        /// <returns>Scheduled tasks</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return this.WrappedTasks;
        }

        #endregion
    }
}