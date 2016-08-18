using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// DrawingToolBase.
/// Base class for all drawing tools.
/// 
/// By Jorge L. Chavez Herrera.
/// </summary>
public class DrawingActionBase : OptimizedGameObject, ISerializationCallbackReceiver
{
    #region Class implementation
    virtual public void Apply() {}
    #endregion

    #region ISerializationCallbackReceiver interface implementation
    virtual public void OnAfterDeserialize() {}

    virtual public void OnBeforeSerialize() {}
    #endregion
}
