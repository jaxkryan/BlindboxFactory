using UnityEngine;

public interface IVisitor {
    void Visit<T>(T visitable) where T : Component, IVisitable;    
}  
