using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Cuts;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Linq;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal abstract class AfterStateMachineWeaveProcessBase : AdviceWeaveProcessBase<AfterAdviceEffect>
    {
        protected TypeDefinition StateMachine;
        protected TypeReference StateMachineRef;

        private readonly Func<FieldReference> _originalThis;

        protected AfterStateMachineWeaveProcessBase(ILogger log, MethodDefinition target, InjectionDefinition injection) : base(log, target, injection)
        {
            _originalThis = _method.IsStatic ? (Func<FieldReference>)null : GetThisField;
        }

        protected void SetStateMachine(TypeReference stateMachineRef)
        {
            StateMachineRef = stateMachineRef;
            StateMachine = StateMachineRef.Resolve();
        }

        private FieldReference GetThisField()
        {
            var thisField = StateMachine.Fields
                .FirstOrDefault(f => f.Name == Constants.MovedThis);

            if (thisField != null) return thisField.MakeReference(StateMachine.MakeSelfReference());
            TypeReference origTypeRef = _type;
            if (origTypeRef.HasGenericParameters)
            {
                origTypeRef = origTypeRef.MakeGenericInstanceType((StateMachine?.GenericParameters?.Take(_type.GenericParameters.Count))?.ToArray());
            }

            thisField = new FieldDefinition(Constants.MovedThis, FieldAttributes.Public, origTypeRef);
            StateMachine.Fields.Add(thisField);

            InsertStateMachineCall(
                e => e
                    .Store(thisField.MakeReference(StateMachineRef), v => v.This())
            );

            return thisField.MakeReference(StateMachine.MakeSelfReference());
        }

        private FieldReference GetArgsField()
        {
            var garfield = StateMachine.Fields
                .FirstOrDefault(f => f.Name == Constants.MovedArgs);

            if (garfield != null) return garfield.MakeReference(StateMachine.MakeSelfReference());
            garfield = new FieldDefinition(Constants.MovedArgs, FieldAttributes.Public, StateMachine.Module.ImportReference(StandardTypes.ObjectArray));
            StateMachine.Fields.Add(garfield);

            InsertStateMachineCall(
                e => e
                    .Store(garfield.MakeReference(StateMachineRef), v =>
                    {
                        var elements = _method.Parameters.Select<ParameterDefinition, PointCut>(p => il =>
                            il.Load(p).Cast(p.ParameterType, StandardTypes.Object)
                        ).ToArray();

                        return v.CreateArray(StandardTypes.Object, elements);
                    }));

            return garfield.MakeReference(StateMachine.MakeSelfReference());
        }

        protected abstract void InsertStateMachineCall(PointCut code);

        public override void Execute()
        {
            if (StateMachineRef == null)
                throw new InvalidOperationException("State machine is not set");

            FindOrCreateAfterStateMachineMethod().Body.BeforeExit(
                e => e
                .LoadAspect(_aspect, _method, LoadOriginalThis)
                .Call(_effect.Method, LoadAdviceArgs)
            );
        }

        protected Cut LoadOriginalThis(Cut pc)
        {
            return _originalThis == null ? pc : pc.This().Load(_originalThis());
        }

        protected override Cut LoadInstanceArgument(Cut pc, AdviceArgument parameter)
        {
            return _originalThis != null ? LoadOriginalThis(pc) : pc.Value(null);
        }

        protected override Cut LoadArgumentsArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.This().Load(GetArgsField());
        }

        protected abstract MethodDefinition FindOrCreateAfterStateMachineMethod();
    }
}
