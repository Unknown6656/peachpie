﻿using System;
using System.Diagnostics;
using Pchp.Core;

public delegate void GeneratorStateMachineDelegate(Context ctx, object @this, PhpArray locals, Generator gen);

[PhpType("Generator")]
public class Generator : Iterator, IDisposable
{
    #region InternalVariables
    /// <summary>
    /// Context associated in which the generator is run.
    /// </summary>
    readonly Context _ctx;

    /// <summary>
    /// Delegate to a static method implementing the state machine itself. 
    /// </summary>
    readonly GeneratorStateMachineDelegate _stateMachineMethod;

    /// <summary>
    /// Bounded this for non-static enumerator methods, null for static ones.
    /// </summary>
    readonly object _this;

    /// <summary>
    /// Lifted local variables from the state machine function.
    /// </summary>
    readonly PhpArray _locals; // Change to internal after all access moved to Operators method

 
    /// <summary>
    /// Current state of the state machine implemented by <see cref="_stateMachineMethod"/>
    /// </summary>
    /// <remarks>
    ///   0: before first yield
    ///  -1: running
    ///  -2: closed
    /// +x: valid state
    /// </remarks>
    public int _state = 0; // Change to internal after all access moved to Operators method

    /// <summary>
    /// Did last yield returned user-specified key.
    /// </summary>
    public bool _userKeyReturned = false; // Change to internal after all access moved to Operators method

    public PhpValue _currValue, _currKey, _currSendItem, _returnValue; // Change to internal after all access moved to Operators method
    public Exception _currException; // Change to internal after all access moved to Operators method
    #endregion

    #region HelperLocalVariables
    /// <summary>
    /// Automatic numerical key for next yield.
    /// </summary>
    long _nextNumericalKey = 0;

    /// <summary>
    /// Helper variables used for <see cref="rewind"/> and <see cref="checkIfRunToFirstYieldIfNotRun"/>
    /// </summary>
    bool _runToFirstYield = false; //Might get replaced by _state logic
    bool _runAfterFirstYield = false;
    #endregion

    #region HelperLocalProperties
    bool isInValidState { get => (_state >= 0); }
    #endregion  

    #region Constructors
    internal Generator(Context ctx, object @this, GeneratorStateMachineDelegate method)
    {
        Debug.Assert(ctx != null);
        Debug.Assert(method != null);

        _stateMachineMethod = method;
        _ctx = ctx;
        _locals = new PhpArray();
        _this = @this;

        _currValue = PhpValue.Null;
        _currKey = PhpValue.Null;
        _currSendItem = PhpValue.Null;
        _returnValue = PhpValue.Null;
    }
    #endregion

    #region IteratorMethods
    /// <summary>
    /// Rewinds the iterator to the first element.
    /// </summary>
    public void rewind()
    {
        checkIfRunToFirstYieldIfNotRun();
        if (_runAfterFirstYield) { throw new Exception("Cannot rewind a generator that was already run"); }
    }

    /// <summary>
    /// Moves forward to next element.
    /// </summary>
    public void next()
    {
        checkIfRunToFirstYieldIfNotRun();
        moveStateMachine();
    }

    /// <summary>
    /// Checks if there is a current element after calls to <see cref="rewind"/> or <see cref="next"/>.
    /// </summary>
    /// <returns><c>bool</c>.</returns>
    public bool valid()
    {
        checkIfRunToFirstYieldIfNotRun();
        return isInValidState;
    }

    /// <summary>
    /// Returns the key of the current element.
    /// </summary>
    public PhpValue key()
    {
        checkIfRunToFirstYieldIfNotRun();
        return _currKey;
    }

    /// <summary>
    /// Returns the current element (value).
    /// </summary>
    public PhpValue current()
    {
        checkIfRunToFirstYieldIfNotRun();
        return _currValue;
    }

    /// <summary>
    /// Get the return value of a generator
    /// </summary>
    /// <returns>Returns the generator's return value once it has finished executing. </returns>
    public PhpValue getReturn()
    {
        if (_state != -2) { throw new Exception("Cannot get return value of a generator that hasn't returned"); }
        return _returnValue;
    }

    /// <summary>
    /// Sends a <paramref name="value"/> to the generator and forwards to next element.
    /// </summary>
    /// <returns>Returns the yielded value. </returns>
    public PhpValue send(PhpValue value)
    {
        checkIfRunToFirstYieldIfNotRun();

        _currSendItem = value;
        moveStateMachine();
        _currSendItem = PhpValue.Null;

        return current();
    }

    /// <summary>
    /// Throw an exception into the generator
    /// </summary>
    /// <param name="ex">Exception to throw into the generator.</param>
    /// <returns>Returns the yielded value. </returns>
    public PhpValue @throw(Exception ex)
    {
        if (!valid()) { throw ex; }

        _currException = ex;
        moveStateMachine();
        _currException = null;

        return current();
    }

    /// <summary>
    /// Serialize callback.
    /// </summary>
    /// <remarks>
    /// Throws an exception as generators can't be serialized. 
    /// </remarks>
    public void __wakeup()
    {
        throw new Exception("Unserialization of 'Generator' is not allowed");
    }
    #endregion

    #region HelperMethods
    /// <summary>
    /// Moves the state machine to next element.
    /// </summary>
    private void moveStateMachine()
    {
        if (!isInValidState) { return; }
        checkIfMovingFromFirstYeild();

        _stateMachineMethod.Invoke(_ctx, _this, _locals, gen: this);

        if (!_userKeyReturned) { _currKey = PhpValue.Create(_nextNumericalKey); }
        if (_currKey.IsInteger()) { _nextNumericalKey = (_currKey.ToLong() + 1); }

        _runToFirstYield = true;

    }

    /// <summary>
    /// Checks if the generator is moving beyond first yield, if so sets proper variable. Important for <see cref="rewind"/>.
    /// </summary>
    private void checkIfMovingFromFirstYeild()
    {
        if (_runToFirstYield && !_runAfterFirstYield) { _runAfterFirstYield = true; }
    }

    /// <summary>
    /// Checks if generator already run to the first yield. Runs there if it didn't.
    /// </summary>
    private void checkIfRunToFirstYieldIfNotRun()
    {
        if (!_runToFirstYield) { this.moveStateMachine(); }
    }

    #endregion

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            disposedValue = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~Generator() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        // GC.SuppressFinalize(this);
    }
    #endregion

}

