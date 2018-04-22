namespace RedBlueGames.MulliganRenamer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class RenameOperationDrawerFactory
    {
        // TODO: DELETE THIS IT DOESN"T WORK>
        public static T Create<T>()
        {
            if (typeof(T) == typeof(AddStringOperationDrawer))
            {
                var addStringDrawer = (IRenameOperationDrawer) new AddStringOperationDrawer();
                return (T) addStringDrawer;
            }

            return default(T);
        }
    }
}