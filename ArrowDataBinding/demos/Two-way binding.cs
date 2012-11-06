using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ArrowDataBinding.demos.TwoWay
{
    public class BindingEventArgs : EventArgs
    {
        public string varName { get; private set; }

        public BindingEventArgs(string varName)
        {
            this.varName = varName;
        }
    }

    [Serializable]
    public class BindableAttribute : LocationInterceptionAspect
    {
        public override void OnSetValue(LocationInterceptionArgs args)
        {
            args.ProceedSetValue();
            ((Bindable)args.Instance).SendUpdate(args.LocationName);
        }
    }

    public abstract class Bindable
    {
        public delegate void BindingUpdateHandler(object sender, BindingEventArgs args);
        public event BindingUpdateHandler valueChanged;

        // Used to mark variables as 'locked' so that assigning to them will not update the binding
        // This prevents infinite loops in two-way bindings
        // Is indexed by variable name (string)
        private Dictionary<string, bool> lockedProperties;

        public Bindable()
        {
            // Add all properties to the locked properties list (initialised to unlocked)
            lockedProperties = new Dictionary<string, bool>();
            var properties = this.GetType().GetProperties();
            // TODO: Make it only use the ones with the bindable attribute?

            foreach (var prop in properties)
            {
                lockedProperties.Add(prop.Name, false);
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

        public void SetVariable<T>(string varName, T val)
        {
            // TODO: this might only work for properties just now! Needs fixing somehow
            this.GetType().GetProperty(varName).SetValue(this, val, null);
        }

        public void LockVariable(string varName)
        {
            // TODO: Modify to work for both fields and properties?
            // TODO: Check for invalid varnames
            lockedProperties[varName] = true;
        }

        public void UnlockVariable(string varName)
        {
            // TODO: All the stuff in LockVariable
            lockedProperties[varName] = false;
        }

        public bool VariableLocked(string varName)
        {
            return lockedProperties[varName];
        }
    }

    public class Binding<T1, T2>
    {
        private Bindable source;
        private string sourceVarName;
        private Bindable destination;
        private string destVarName;

        public Binding(Bindable source, string sourceVar, Bindable dest, string destVar)
        {
            this.source = source;
            this.sourceVarName = sourceVar;
            this.destination = dest;
            this.destVarName = destVar;

            source.valueChanged += NotifyChange;
            dest.valueChanged += NotifyChange;
        }

        public void NotifyChange(object source, BindingEventArgs args)
        {
            Bindable changedObj, updateObj;
            string sourceVar, destVar;

            if (source.Equals(this.source))
            {
                changedObj = this.source;
                updateObj = this.destination;
                sourceVar = this.sourceVarName;
                destVar = this.destVarName;
            }
            else
            {
                changedObj = this.destination;
                updateObj = this.source;
                sourceVar = this.destVarName;
                destVar = this.sourceVarName;
            }

            // First check whether the target variable is locked
            if (updateObj.VariableLocked(destVar)) return;

            // Currently wrong as it forces the source variable to be a T2
            Type t2 = typeof(T2);
            PropertyInfo info = changedObj.GetType().GetProperty(sourceVar);
            T2 thingy = (T2)info.GetValue(changedObj, null);

            // Lock the variable (to avoid a recursive update once it has been set) and update
            updateObj.LockVariable(destVar);
            updateObj.SetVariable<T2>(destVar, (T2)changedObj.GetType().GetProperty(sourceVar).GetValue(changedObj, null));
            updateObj.UnlockVariable(destVar);
        }
    }

    public class BindingManager
    {
        public static void CreateBinding(Bindable source, string sourceVar, Bindable destination, string destVar)
        {
            // Now create a binding by getting the types of each variable, creating a genericised
            // binding constructor and rolling onwards
            Type sourceType = source.GetType().GetProperty(sourceVar).PropertyType;
            Type destType = destination.GetType().GetProperty(destVar).PropertyType;

            Type[] bindingConstructorTypes = new Type[] {
                typeof(Bindable), typeof(string), typeof(Bindable), typeof(string)
            };

            ConstructorInfo bindingConstructor = typeof(Binding<,>).MakeGenericType(sourceType, destType).GetConstructor(bindingConstructorTypes);

            dynamic[] parameters = new dynamic[] {
                source, sourceVar, destination, destVar
            };

            dynamic binding = bindingConstructor.Invoke(parameters);
        }
    }


    public class IntegerLeft : Bindable
    {
        [Bindable]
        public int magic { get; set; }

        public IntegerLeft(int m) : base()
        {
            magic = m;
        }
    }

    public class IntegerRight : Bindable
    {
        [Bindable]
        public int spiffy { get; set; }

        public IntegerRight(int s) : base()
        {
            spiffy = s;
        }
    }

    class Third : Bindable
    {
        [Bindable]
        public int third { get; set; }
    }
}
