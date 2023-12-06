using Lotus.Roles.Interactions.Interfaces;
using Lotus.Extensions;
using Lotus.Roles2;

namespace Lotus.Roles.Interactions;

public class LotusInteraction : Interaction
{
    public static Stub FatalInteraction = new(new FatalIntent());
    public static Stub HostileInteraction = new(new HostileIntent());
    public static Stub NeutralInteraction = new(new NeutralIntent());
    public static Stub HelpfulInteraction = new(new HelpfulIntent());

    private UnifiedRoleDefinition unifiedRoleDefinition;

    public LotusInteraction(Intent intent, UnifiedRoleDefinition roleDefinition)
    {
        this.Intent = intent;
        this.unifiedRoleDefinition = roleDefinition;
    }

    public LotusInteraction(Intent intent, RoleDefinition roleDefinition)
    {
        this.Intent = intent;
        this.unifiedRoleDefinition = roleDefinition.Handle;
    }

    public UnifiedRoleDefinition Emitter() => unifiedRoleDefinition;
    public Intent Intent { get; set; }
    public bool IsPromised { get; set; }

    public virtual Interaction Modify(Intent intent) => new LotusInteraction(intent, unifiedRoleDefinition);

    public class Stub
    {
        private Intent intent;
        public Stub(Intent intent)
        {
            this.intent = intent;
        }

        public LotusInteraction Create(UnifiedRoleDefinition role)
        {
            return new LotusInteraction(intent, role);
        }

        public LotusInteraction Create(RoleDefinition definition)
        {
            return new LotusInteraction(intent, definition);
        }

        public LotusInteraction Create(PlayerControl player)
        {
            return new LotusInteraction(intent, player.PrimaryRole());
        }
    }
}