namespace ObjectBinderEditor
{
    public interface IEventRule
    {
        bool IsMatch(ObjectBinder.Item item);
        string GenerateEventMethodSignature(ObjectBinder.Item item);
        string GenerateEventCode(ObjectBinder.Item item);
    }

}