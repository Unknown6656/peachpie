﻿using MySql.Data.MySqlClient;
using Pchp.Core;
using Pchp.Library.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using Pchp.Library.Resources;

namespace Peachpie.Library.MySql
{
    /// <summary>
    /// Resource representing MySql connection.
    /// </summary>
    sealed class MySqlConnectionResource : ConnectionResource
    {
        const string ResourceName = "mysql connection";

        readonly MySqlConnectionManager _manager;
        readonly MySqlConnection _connection;

        /// <summary>
        /// Lazily set server name used to initiate connection.
        /// </summary>
        internal string Server { get; set; }

        /// <summary>
        /// Gets associated runtime <see cref="Context"/>.
        /// </summary>
        internal Context Context => _manager.Context;

        public MySqlConnectionResource(MySqlConnectionManager manager, string connectionString)
            : base(connectionString, ResourceName)
        {
            _manager = manager;
            _connection = new MySqlConnection(this.ConnectionString);
        }

        protected override void FreeManaged()
        {
            base.FreeManaged();
            _manager.RemoveConnection(this);
        }

        public override void ClosePendingReader()
        {
            _pendingReader?.Dispose();
            _pendingReader = null;
        }

        protected override IDbConnection ActiveConnection => _connection;

        protected override ResultResource GetResult(IDataReader reader, bool convertTypes)
        {
            return new MySqlResultResource(this, reader, convertTypes);
        }

        protected override IDbCommand CreateCommand(string commandText, CommandType commandType) => CreateCommandInternal(commandText, commandType);

        internal MySqlCommand CreateCommandInternal(string commandText, CommandType commandType = CommandType.Text)
        {
            return new MySqlCommand()
            {
                Connection = _connection,
                CommandText = commandText,
                CommandType = commandType
            };
        }

        internal ResultResource ExecuteCommandInternal(IDbCommand command, bool convertTypes, IList<IDataParameter> parameters, bool skipResults)
        {
            return ExecuteCommandProtected(command, convertTypes, parameters, skipResults);
        }

        /// <summary>
        /// Gets the server version.
        /// </summary>
        internal string ServerVersion => _connection.ServerVersion;

        /// <summary>
        /// Returns the id of the server thread this connection is executing on.
        /// </summary>
        internal int ServerThread => _connection.ServerThread;

        /// <summary>
        /// Pings the server.
        /// </summary>
        internal bool Ping()
        {
            return _connection.Ping();
        }

        /// <summary>
		/// Queries server for a value of a global variable.
		/// </summary>
		/// <param name="name">Global variable name.</param>
		/// <returns>Global variable value (converted).</returns>
		internal object QueryGlobalVariable(string name)
        {
            // TODO: better query:

            var result = ExecuteQuery("SHOW GLOBAL VARIABLES LIKE '" + name + "'", true);

            // default value
            if (result.FieldCount != 2 || result.RowCount != 1)
            {
                return null;
            }

            return result.GetFieldValue(0, 1);
        }

        /// <summary>
        /// Gets last inserted row autogenerated ID if applicable, otherwise <c>-1</c>.
        /// </summary>
        internal long LastInsertedId
        {
            get
            {
                var command = (MySqlCommand)LastResult?.Command;
                return command != null
                    ? command.LastInsertedId
                    : -1L;
            }
        }
    }
}
