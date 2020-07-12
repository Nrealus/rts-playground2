using System;

namespace Nrealus.Extensions.ReferenceWrapper
{
    
    public class RefWrapperExample : RefWrapper<WrappedObjectExample>
    {
        public RefWrapperExample(WrappedObjectExample wrappedObject, Action nullifyPrivateRefToWrapper) : base(wrappedObject, nullifyPrivateRefToWrapper)
        {}
    }

    public class WrappedObjectExample
    {
        protected RefWrapperExample _myWrapper;
        public RefWrapperExample GetRefWrapper() { return _myWrapper; }

        public WrappedObjectExample()
        {
            _myWrapper = new RefWrapperExample(this, () => _myWrapper = null );
        }

        public void OnDestroy()
        {
            GetRefWrapper().DestroyWrappedReference();
        }
    }

}