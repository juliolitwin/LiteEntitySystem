﻿using LiteEntitySystem.Internal;

namespace LiteEntitySystem
{
    /// <summary>
    /// Base class for Controller entities
    /// </summary>
    [EntityFlags(EntityFlags.OnlyForOwner)]
    public abstract class ControllerLogic : InternalEntity
    {
        [SyncVarFlags(SyncFlags.NeverRollBack)]
        private SyncVar<EntitySharedReference> _controlledEntity;
        
        /// <summary>
        /// Is controller - AI controller
        /// </summary>
        public abstract bool IsBot { get; }

        public T GetControlledEntity<T>() where T : PawnLogic =>
            EntityManager.GetEntityById<T>(_controlledEntity);

        /// <summary>
        /// Called before controlled entity update. Useful for input
        /// </summary>
        protected internal virtual void BeforeControlledUpdate()
        {
            
        }

        /// <summary>
        /// Destroy controller and controlled pawn
        /// </summary>
        public void DestroyWithControlledEntity()
        {
            GetControlledEntity<PawnLogic>()?.Destroy();
            _controlledEntity.Value = null;
            Destroy();
        }

        /// <summary>
        /// Start control pawn
        /// </summary>
        /// <param name="target"></param>
        public void StartControl(PawnLogic target)
        {
            if (IsClient)
                return;
            StopControl();
            _controlledEntity.Value = target;
            GetControlledEntity<PawnLogic>().Controller = this;
        }

        internal override void DestroyInternal()
        {
            StopControl();
            base.DestroyInternal();
        }

        /// <summary>
        /// Stop control pawn
        /// </summary>
        public void StopControl()
        {
            if (IsClient)
                return;
            var controlledLogic = GetControlledEntity<PawnLogic>();
            if (controlledLogic == null)
                return;
            controlledLogic.Controller = null;
            _controlledEntity.Value = null;
        }
        
        protected ControllerLogic(EntityParams entityParams) : base(entityParams) { }
    }

    /// <summary>
    /// Base class for AI Controller entities
    /// </summary>
    [EntityFlags(EntityFlags.Updateable)]
    public abstract class AiControllerLogic : ControllerLogic
    {
        public override bool IsBot => true;
        
        protected AiControllerLogic(EntityParams entityParams) : base(entityParams) { }
    }

    /// <summary>
    /// Base class for AI Controller entities with typed ControlledEntity field
    /// </summary>
    [EntityFlags(EntityFlags.Updateable)]
    public abstract class AiControllerLogic<T> : AiControllerLogic where T : PawnLogic
    {
        public T ControlledEntity => GetControlledEntity<T>();
        
        protected AiControllerLogic(EntityParams entityParams) : base(entityParams) { }
    }
}