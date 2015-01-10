using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
namespace SimpleTriangle.Util
{
    /// <summary>
    /// Représente un pool de threads à exécuter.
    /// </summary>
    public class ThreadPool
    {
        public int MaxRunningThreads { get; set; }
        /// <summary>
        /// Contient la file des threads en attente.
        /// </summary>
        public List<Thread> m_waitingThreads;
        /// <summary>
        /// Contient la liste des threads en cours d'exécution.
        /// </summary>
        public List<Thread> m_currentThreads;


        /// <summary>
        /// Crée une nouvelle instance du pool de threads.
        /// </summary>
        public ThreadPool()
        {
            m_waitingThreads = new List<Thread>();
            m_currentThreads = new List<Thread>();
            MaxRunningThreads = 3;
        }

        /// <summary>
        /// Ajoute un thread dans la pool de threads.
        /// </summary>
        /// <param name="thread"></param>
        public void AddThread(Thread thread)
        {
            m_waitingThreads.Insert(0, thread);
        }

        /// <summary>
        /// Supprime un thread de la pool de threads.
        /// </summary>
        /// <param name="thread"></param>
        public void RemoveThread(Thread thread)
        {
            m_waitingThreads.Remove(thread);
        }

        /// <summary>
        /// Mets à jour la pool de threads.
        /// </summary>
        public void Update()
        {
            List<Thread> toDelete = new List<Thread>();
            foreach(Thread thread in m_currentThreads)
            {
                if(thread.ThreadState == ThreadState.Stopped)
                {
                    toDelete.Add(thread);
                }
            }
            // Supprime les threads terminés.
            foreach(Thread thread in toDelete)
            {
                m_currentThreads.Remove(thread);
            }

            // Lance les threads si la pile est non vide.
            while(m_currentThreads.Count < MaxRunningThreads && m_waitingThreads.Count > 0)
            {
                Thread newThread = m_waitingThreads.Last();
                m_waitingThreads.Remove(newThread);

                newThread.Start();
                m_currentThreads.Add(newThread);
            }

            Scene.Instance.DiagnosisWindow.ThreadCount = m_currentThreads.Count;
            Scene.Instance.DiagnosisWindow.WaitingThreadCount = m_waitingThreads.Count;
        }
    }
}
