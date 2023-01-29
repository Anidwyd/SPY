using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class Timer : MonoBehaviour
{
    private DateTime _startTime;
    private TimeSpan _elapsedTime = new TimeSpan();
    private bool _isRunning = false;



    public void Start()
    {
        if (!_isRunning)
        {
            _startTime = DateTime.Now;
            _isRunning = true;
        }
    }

    public void Stop()
    {
        if (_isRunning)
        {
            _elapsedTime = Elapsed;
            _isRunning = false;
        }
    }

    public void Reset()
    {
        _elapsedTime = new TimeSpan();
        _isRunning = false;
    }

    public TimeSpan Elapsed
    {
        get
        {
            if (_isRunning)
            {
                return DateTime.Now - _startTime + _elapsedTime;
            }
            else
            {
                return _elapsedTime;
            }
        }
    }


}
