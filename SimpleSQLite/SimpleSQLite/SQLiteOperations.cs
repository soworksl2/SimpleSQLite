using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace SimpleSQLite
{
    /// <summary>
    /// Proporciona Metodos y funciones para trabajar con bases de datos SQLite
    /// </summary>
    public static class SQLiteOperations
    {
        #region Metodos Publicos

        /// <summary>
        /// Cuenta y devuelve todos los registros de una tabla que cumplan con una expresion
        /// </summary>
        /// <typeparam name="T">El tipo del modelo que representa la tabla</typeparam>
        /// <param name="expression">La expresion que el modelo debe cumplir para ser contado</param>
        /// <returns>La cantidad de registros que cumplan con el criterio</returns>
        public static int Count<T>(string fullPathDB = "", Expression<Func<T, bool>> expression = null)
        {
            int output;

            using (SQLiteConnection connection = GetConnection(fullPathDB))
            {
                connection.CreateTable<T>();

                TableQuery<T> TQ = new TableQuery<T>(connection);

                if (expression == null)
                    output = TQ.Count();
                else
                    output = TQ.Count(expression);
            }
            return output;
        }

        /// <summary>
        /// Inserta el modelo correspondiente en la base de datos indicada
        /// </summary>
        /// <typeparam name="T">El tipo del modelo a ingresar</typeparam>
        /// <param name="element">Elemento a introducir en la base de datos</param>
        /// <param name="fullPathDB">La direccion Completa o relativa hacia la base de datos. dejar vacio para indicar ruta por defecto</param>
        public static void Insert<T>(T element, string fullPathDB = "")
        {
            using (SQLiteConnection connection = GetConnection(fullPathDB))
            {
                connection.CreateTable<T>();
                connection.Insert(element);
            }
            OnAfterExecuteOperation?.Invoke(new List<object>() { element }, ReflectionExtensions.GetTableName(typeof(T)), SQLiteOperation.Insert);
        }
        /// <summary>
        /// Inserta todos los modelos correspondientes en la base de datos indicada
        /// </summary>
        /// <typeparam name="T">El tipo de los modelos a ingresar</typeparam>
        /// <param name="elements">Los elementos a ingresar en la base de datos</param>
        /// <param name="fullPathDB">La direccion completa o parcial hacia la base de datos. Dejar vacio para inicar ruta por defecto</param>
        public static void Insert<T>(IEnumerable<T> elements, string fullPathDB = "")
        {
            using (SQLiteConnection connection = GetConnection(fullPathDB))
            {
                connection.CreateTable<T>();
                connection.InsertAll(elements);
            }
            OnAfterExecuteOperation?.Invoke(elements.Cast<Object>(), ReflectionExtensions.GetTableName(typeof(T)), SQLiteOperation.Insert);
        }

        /// <summary>
        /// Recupera el registro de la base de datos correspondiente segun el modelo por su PrimaryKey
        /// </summary>
        /// <typeparam name="T">El tipo del modelo que se va a leer</typeparam>
        /// <param name="primaryKey">El id del modelo en la base de datos correspondiente</param>
        /// <param name="fullPathDB">La ruta completa o relativa hacia la base de datos</param>
        /// <param name="readWithChildren">Si se desea rescatar las relaciones del modelo</param>
        /// <param name="recursive">Si se desea que el rescatado de las relaciones sea recursivo</param>
        /// <returns>El modelo correspondiente</returns>
        public static T Read<T>(object primaryKey, string fullPathDB = "", bool readWithChildren = false, bool recursive = false) where T : new()
        {
            T output;

            using (SQLiteConnection connection = GetConnection(fullPathDB))
            {
                connection.CreateTable<T>();

                if (readWithChildren)
                    output = connection.FindWithChildren<T>(primaryKey, recursive);
                else
                    output = connection.Find<T>(primaryKey);
            }
            return output;
        }
        /// <summary>
        /// Recupera todos los registros de la base de datos que cumplan con un criterio
        /// </summary>
        /// <typeparam name="T">El tipo del modelo que se buscara en la base de datos</typeparam>
        /// <param name="expression">El criterio que deben cumplir los modelos para ser seleccionados. null para seleccionar todos</param>
        /// <param name="fullPathDB">La ruta completa o relativa hacia la base de datos</param>
        /// <returns>Todos los modelos que cumplan con el criterio</returns>
        public static IEnumerable<T> Read<T>(Expression<Func<T, bool>> expression = null, string fullPathDB = "")
        {
            IEnumerable<T> output;

            using (SQLiteConnection connection = GetConnection(fullPathDB))
            {
                connection.CreateTable<T>();

                TableQuery<T> TQ = new TableQuery<T>(connection);

                if (expression == null)
                    output = TQ.ToList();
                else
                    output = TQ.Where(expression).ToList();
            }
            return output;
        }

        /// <summary>
        /// Actualiza un modelo en la base de datos correspondiente
        /// </summary>
        /// <typeparam name="T">El tipo del modelo a actualizar</typeparam>
        /// <param name="elementToUpdate">El elemento ya actualizado</param>
        /// <param name="fullPathDB">La ruta completa o relativa hacia la base de datos</param>
        public static void Update<T>(T elementToUpdate, string fullPathDB = "")
        {
            using (SQLiteConnection connection = GetConnection(fullPathDB))
            {
                connection.Update(elementToUpdate);
            }
            OnAfterExecuteOperation?.Invoke(new List<object>() { elementToUpdate }, ReflectionExtensions.GetTableName(typeof(T)), SQLiteOperation.Update);
        }
        /// <summary>
        /// Actualiza todos los modelos en la base de datos correspondiente
        /// </summary>
        /// <typeparam name="T">El tipo de los modelos</typeparam>
        /// <param name="elementsToUpdate">Los modelos que se actualizaran</param>
        /// <param name="fullPathDB">La ruta completa o relativa hacia la base de datos</param>
        public static void Update<T>(IEnumerable<T> elementsToUpdate, string fullPathDB = "")
        {
            using (SQLiteConnection connection = GetConnection(fullPathDB))
            {
                connection.UpdateAll(elementsToUpdate);
            }
            OnAfterExecuteOperation?.Invoke(elementsToUpdate.Cast<object>(), ReflectionExtensions.GetTableName(typeof(T)), SQLiteOperation.Update);
        }

        /// <summary>
        /// Elimina un modelo de la base de datos correspondiente segun su ID
        /// </summary>
        /// <typeparam name="T">El tipo del modelo a eliminar</typeparam>
        /// <param name="elementToDelete">El modelo que se quiere eliminar de la base de datos</param>
        /// <param name="fullPathDB">La ruta completa o relativa hacia la base de datos</param>
        /// <param name="recursive">Decide si se quiere eliminar el objeto con todos sus hijos de manera recursiva. Eliminacion en cascada</param>
        public static void Delete<T>(T elementToDelete, string fullPathDB = "", bool recursive = false)
        {
            using (SQLiteConnection connection = GetConnection(fullPathDB))
            {
                connection.Delete(elementToDelete, recursive);
            }
            OnAfterExecuteOperation?.Invoke(new List<object>() { elementToDelete }, ReflectionExtensions.GetTableName(typeof(T)), SQLiteOperation.Delete);
        }
        /// <summary>
        /// Elimina los modelos correspondientes de la base de datos correspondientes
        /// </summary>
        /// <typeparam name="T">El tipo de los modelos a eliminar</typeparam>
        /// <param name="elementsToDelete">Los modelos a eliminar</param>
        /// <param name="fullPathDB">La ruta completa o relativa hacia la base de datos</param>
        /// <param name="recursive">Si se desea eliminar los modelos de la base de datos de manera recursiva. Eliminacion en cascada</param>
        public static void Delete<T>(IEnumerable<T> elementsToDelete, string fullPathDB = "", bool recursive = false)
        {
            using(SQLiteConnection connection = GetConnection(fullPathDB))
            {
                foreach (T element in elementsToDelete)
                    connection.Delete(element, recursive);
            }
            OnAfterExecuteOperation?.Invoke(elementsToDelete.Cast<object>(), ReflectionExtensions.GetTableName(typeof(T)), SQLiteOperation.Delete);
        }

        /// <summary>
        /// Rellena al modelo con todas sus relaciones
        /// </summary>
        /// <typeparam name="T">El tipo del modelo</typeparam>
        /// <param name="elementToFill">El elemento que se rellenara</param>
        /// <param name="fullPathDB">La ruta completa o relativa hacia la base de datos de los modelos</param>
        /// <param name="recursive">Si se desea obtener las ralaciones recursivamente</param>
        public static void FillWithChildren<T>(T elementToFill, string fullPathDB = "", bool recursive = false)
        {
            using(SQLiteConnection connection = GetConnection(fullPathDB))
            {
                connection.GetChildren<T>(elementToFill, recursive);
            }
        }

        #endregion

        #region Metodos Privados

        private static SQLiteConnection GetConnection(string pathDb)
        {
            //Si es nulo o vacio regresa una ruta default
            if (string.IsNullOrWhiteSpace(pathDb))
                return new SQLiteConnection(Path.GetFullPath("./DB.db3"));

            pathDb = Path.GetFullPath(pathDb);

            //Si el directorio hacia la base de datos no existe.
            if (!Directory.Exists(Path.GetDirectoryName(pathDb)))
                throw new Exception("El directorio para llegar a la base de datos no existe");

            return new SQLiteConnection(pathDb);
        }

        #endregion

        #region Eventos

        /// <summary>
        /// Se lanza despues de que se realiza una operacion hacia alguna base de datos. El evento como parametros devuelve todos los registros que participaron en la operacion, El nombre de la tabla del modelo en la BD y la operacion que realizaron respectivamente
        /// </summary>
        public static event Action<IEnumerable<object>, string, SQLiteOperation> OnAfterExecuteOperation;

        #endregion
    }

    /// <summary>
    /// Indica una operacion de base de datos
    /// </summary>
    public enum SQLiteOperation
    {
        Insert, Update, Delete
    }
}
