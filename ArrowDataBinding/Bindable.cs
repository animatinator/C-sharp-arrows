using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostSharp.Aspects;
using System.Reflection;

namespace ArrowDataBinding.Bindings
{
    public class BindingEventArgs : EventArgs
    {
        /*
         * The argument object to be sent by a Bindable object whenever one of its bound variables
         * changes - contains the name of the variable.
         */

        public string VarName { get; private set; }

        public BindingEventArgs(string varName)
        {
            this.VarName = varName;
        }
    }

    public class BoundVariableNotFoundException : Exception
    {
        public BoundVariableNotFoundException(string varName, string sourceObjName)
            : base(String.Format("The variable '{0}' could not be found in the target object '{1}'.",
            varName, sourceObjName))
        { }
    }

    public class ObjectNotBindableException : Exception
    {
        public ObjectNotBindableException(string sourceObjName)
            : base(String.Format("The object of type '{0}' does not extend Bindable and cannot " +
            "be bound to.", sourceObjName))
        { }
    }

    public class VariableLockedException : Exception
    {
        public VariableLockedException(string varName, string sourceObjName)
            : base(String.Format("The variable '{0}' belonging to the object of type '{1}' is " +
            "locked by another binding and cannot be set.", varName, sourceObjName))
        { }
    }


    public struct BindPoint
    {
        /*
         * Simple struct holding the details of a binding point - the object being bound, and the
         * name of the specific member contained within it
         * Used in creating bindings as it keeps the object and varname together conveniently and
         * simplifies syntax
         */

        public Bindable Object { get; private set; }
        public string Var { get; private set; }

        public BindPoint(Bindable obj, string var) : this()
        {
            if (obj.HasVariable(var))
            {
                this.Object = obj;
                this.Var = var;
            }
            else
            {
                throw new BoundVariableNotFoundException(var, obj.GetType().ToString());
            }
        }
    }


    [Serializable]
    public class BindableAttribute : LocationInterceptionAspect
    {
        /*
         * A PostSharp attribute which is used to intercept assignments to a member variable or
         * property and call the SendUpdate(variable_name) method which the containing object
         * will have inherited from Bindable.
         */

        public override void OnSetValue(LocationInterceptionArgs args)
        {
            args.ProceedSetValue();
            ((Bindable)args.Instance).SendUpdate(args.LocationName);
        }
    }

    public abstract class Bindable
    {
        /*
         * Provides the set of utility functions which an object being bound to will need. Most
         * importantly, it provides the SendUpdate(variable_name) method and valueChanged event
         * which are both used to notify subscribers of a variable changing value, along with
         * methods to get and set variables by name. It also handles the locking and unlocking
         * of variables which prevents infinite loops in two-way bindings.
         */

        public delegate void BindingUpdateHandler(object sender, BindingEventArgs args);
        public event BindingUpdateHandler valueChanged;

        // Used to mark variables as 'locked' so that assigning to them will not update the binding
        // This prevents infinite loops in two-way bindings
        // Is indexed by variable name (string)
        private Dictionary<string, bool> lockedMembers;

        public Bindable()
        {
            InitialiseLockedMembersList();
        }

        private void InitialiseLockedMembersList()
        {
            // Add all properties to the locked properties list (initialised to unlocked)
            lockedMembers = new Dictionary<string, bool>();
            var bindableVars = GetFieldsAndProperties();
            // TODO: Make it only use the ones with the bindable attribute?

            foreach (var variable in bindableVars)
            {
                lockedMembers.Add(variable.Name, false);
            }
        }

        public void SendUpdate(string varName)
        {
            BindingEventArgs args = new BindingEventArgs(varName);

            if (valueChanged != null)
            {
                valueChanged(this, args);
            }
        }


        public void LockAndSetVariable<T>(string varName, T val)
        {
            /*
             * This is used to prevent recursive updating when a cycle or two-way binding exists.
             * When a variable is locked, the SetVariable method will refuse to set it and throw
             * an exception instead.
             */

            LockVariable(varName);
            SetVariable<T>(varName, val);
            UnlockVariable(varName);
        }

        public void SetVariable<T>(string varName, T val)
        {
            if (VariableLocked(varName)) throw new VariableLockedException(varName, this.GetType().ToString());

            if (this.HasProperty(varName)) this.SetProperty<T>(varName, val);
            else if (this.HasField(varName)) this.SetField<T>(varName, val);
            else throw new BoundVariableNotFoundException(varName, this.GetType().ToString());
        }

        public T GetVariable<T>(string varName)
        {
            if (this.HasProperty(varName)) return this.GetProperty<T>(varName);
            else if (this.HasField(varName)) return this.GetField<T>(varName);
            else throw new BoundVariableNotFoundException(varName, this.GetType().ToString());
        }

        //TODO: Maybe the different setters and getters for properties and fields could use the Template pattern or similar to avoid code duplication?
        private void SetProperty<T>(string propName, T val)
        {
            Type type = this.GetType();
            type.GetProperty(propName).SetValue(this, val, null);
        }

        private T GetProperty<T>(string propName)
        {
            Type type = this.GetType();
            return (T)type.GetProperty(propName).GetValue(this, null);
        }

        private void SetField<T>(string propName, T val)
        {
            Type type = this.GetType();
            type.GetField(propName).SetValue(this, val);
        }

        private T GetField<T>(string propName)
        {
            Type type = this.GetType();
            return (T)type.GetField(propName).GetValue(this);
        }


        public void LockVariable(string varName)
        {
            lockedMembers[varName] = true;
        }

        public void UnlockVariable(string varName)
        {
            lockedMembers[varName] = false;
        }

        public bool VariableLocked(string varName)
        {
            return lockedMembers[varName];
        }


        public bool HasVariable(string varName)
        {
            return (this.HasProperty(varName) || this.HasField(varName));
        }
        
        private bool HasProperty(string propName)
        {
            Type type = this.GetType();
            return (type.GetProperty(propName) != null);
        }

        private bool HasField(string fieldName)
        {
            Type type = this.GetType();
            return (type.GetProperty(fieldName) != null);
        }


        private List<MemberInfo> GetFieldsAndProperties()
        {
            List<MemberInfo> members = new List<MemberInfo>();

            members.AddRange(GetFields());
            members.AddRange(GetProperties());

            return members;
        }

        private List<MemberInfo> GetFields()
        {
            List<MemberInfo> fields = new List<MemberInfo>();
            fields.AddRange(this.GetType().GetFields());
            return fields;
        }

        private List<MemberInfo> GetProperties()
        {
            List<MemberInfo> properties = new List<MemberInfo>();
            properties.AddRange(this.GetType().GetProperties());
            return properties;
        }
    }
}
