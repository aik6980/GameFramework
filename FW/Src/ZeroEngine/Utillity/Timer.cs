using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroEngine.Utillity
{
    class CAccumTimer
    {
        DateTime m_StartTime;
        DateTime m_CurrTime;
        DateTime m_LastTime;

        public void Start()
        {
            Start(DateTime.Now);
        }

        public void Start(DateTime startTime)
        {
            m_StartTime = m_CurrTime = startTime;
        }

        public void Tick()
        {
            Tick(DateTime.Now);
        }

        public void Tick(DateTime currTime)
        {
            m_LastTime = m_CurrTime;
            m_CurrTime = currTime;
        }

        public float GetIntervalMS()
        {
            TimeSpan interval = m_CurrTime - m_LastTime;
            return (float)interval.TotalMilliseconds;
        }

        public float GetElapsedTimeMS()
        {
            TimeSpan interval = m_CurrTime - m_StartTime;
            return (float)interval.TotalMilliseconds;
        }
    }
}
