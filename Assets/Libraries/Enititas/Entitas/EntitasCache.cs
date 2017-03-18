using System.Collections.Generic;

namespace Entitas {

    public static class EntitasCache {

        static readonly ObjectCache _cache = new ObjectCache();

        public static List<IComponent> GetIComponentList() { return _cache.Get<List<IComponent>>(); }
        public static void PushIComponentList(List<IComponent> list) { list.Clear(); _cache.Push(list); }

        public static List<int> GetIntList() { return _cache.Get<List<int>>(); }
        public static void PushIntList(List<int> list) { list.Clear(); _cache.Push(list); }

        public static HashSet<int> GetIntHashSet() { return _cache.Get<HashSet<int>>(); }
        public static void PushIntHashSet(HashSet<int> hashSet) { hashSet.Clear(); _cache.Push(hashSet); }

        public static List<GroupChanged<TEntity>> GetGroupChangedList<TEntity>() where TEntity : class, IEntity, new() { return _cache.Get<List<GroupChanged<TEntity>>>(); }
        public static void PushGroupChangedList<TEntity>(List<GroupChanged<TEntity>> list) where TEntity : class, IEntity, new() { list.Clear(); _cache.Push(list); }

        public static void Reset() {
            _cache.Reset();
        }
    }
}
