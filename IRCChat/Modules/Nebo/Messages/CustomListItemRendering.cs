

namespace MetaBuilders.Irc.Messages
{
    /// <summary>
    /// A delegate which provides custom format rendering for the items in a list.
    /// </summary>
    public delegate string CustomListItemRendering<T> (T item);
}
