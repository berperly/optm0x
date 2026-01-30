#nullable enable

namespace _0xOptimizer.Core.Operations
{
    public interface IOperation
    {
        string Name { get; }

        // Se true e o app não estiver admin, o runner pode bloquear e avisar.
        bool RequiresAdmin { get; }

        // Aplica o tweak (sem retornar bool pra forçar log detalhado no ctx)
        void Apply(IOperationContext ctx);

        // Best-effort: confirma se foi aplicado (quando der)
        bool CheckApplied();
    }
}