using System.Collections.Generic;

namespace Treynessen.Security.Installation;

internal static class CatalogQueries
{
    // [Комментарий ИИ-агента]: Только фиксированные запросы к pg_catalog; имена схемы передаются параметром, пользовательский SQL не принимается.
    internal static readonly IReadOnlyDictionary<string, string> All = new Dictionary<string, string>
    {
        ["relations"] = """
            SELECT jsonb_build_object('table',c.relname,'kind',c.relkind::text,'persistence',c.relpersistence::text,
              'partition',c.relispartition,'rls',c.relrowsecurity,'forceRls',c.relforcerowsecurity,
              'method',am.amname,'inherits',c.relhassubclass,'ownerIsCurrent',c.relowner=(SELECT oid FROM pg_catalog.pg_roles WHERE rolname=current_user))
            FROM pg_catalog.pg_class c JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
            LEFT JOIN pg_catalog.pg_am am ON am.oid=c.relam
            WHERE n.nspname=@schema AND c.relkind NOT IN ('i','I','S')
            """,
        ["columns"] = """
            SELECT jsonb_build_object('table',c.relname,'column',a.attname,'ordinal',a.attnum,
              'type',pg_catalog.format_type(a.atttypid,a.atttypmod),'typeSchema',tn.nspname,
              'notNull',a.attnotnull,'identity',a.attidentity::text,'generated',a.attgenerated::text,
              'dropped',a.attisdropped,'default',pg_catalog.pg_get_expr(d.adbin,d.adrelid,false),
              'collation',CASE WHEN co.oid IS NULL THEN NULL ELSE cn.nspname||'.'||co.collname END)
            FROM pg_catalog.pg_class c JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace
            JOIN pg_catalog.pg_attribute a ON a.attrelid=c.oid AND a.attnum>0
            JOIN pg_catalog.pg_type t ON t.oid=a.atttypid JOIN pg_catalog.pg_namespace tn ON tn.oid=t.typnamespace
            LEFT JOIN pg_catalog.pg_attrdef d ON d.adrelid=c.oid AND d.adnum=a.attnum
            LEFT JOIN pg_catalog.pg_collation co ON co.oid=a.attcollation LEFT JOIN pg_catalog.pg_namespace cn ON cn.oid=co.collnamespace
            WHERE n.nspname=@schema AND c.relkind NOT IN ('i','I','S','t')
            """,
        ["constraints"] = """
            SELECT jsonb_build_object('table',cl.relname,'kind',co.contype::text,
              'columns',ARRAY(SELECT a.attname FROM unnest(co.conkey) WITH ORDINALITY k(num,pos) JOIN pg_catalog.pg_attribute a ON a.attrelid=co.conrelid AND a.attnum=k.num ORDER BY k.pos),
              'principalSchema',pn.nspname,'principal',pc.relname,
              'principalColumns',ARRAY(SELECT a.attname FROM unnest(co.confkey) WITH ORDINALITY k(num,pos) JOIN pg_catalog.pg_attribute a ON a.attrelid=co.confrelid AND a.attnum=k.num ORDER BY k.pos),
              'delete',co.confdeltype::text,'update',co.confupdtype::text,'match',co.confmatchtype::text,
              'deferrable',co.condeferrable,'deferred',co.condeferred,'validated',co.convalidated,'enforced',co.conenforced,
              'period',co.conperiod,'parent',co.conparentid<>0,'local',co.conislocal,'ancestors',co.coninhcount,
              'noInherit',co.connoinherit,'expression',CASE WHEN co.contype='c' THEN pg_catalog.pg_get_constraintdef(co.oid,false) ELSE NULL END)
            FROM pg_catalog.pg_constraint co JOIN pg_catalog.pg_class cl ON cl.oid=co.conrelid
            JOIN pg_catalog.pg_namespace n ON n.oid=cl.relnamespace
            LEFT JOIN pg_catalog.pg_class pc ON pc.oid=co.confrelid LEFT JOIN pg_catalog.pg_namespace pn ON pn.oid=pc.relnamespace
            WHERE n.nspname=@schema AND co.contype<>'n'
            """,
        ["nullConstraints"] = """
            SELECT jsonb_build_object('table',cl.relname,'column',a.attname,
              'enforced',co.conenforced,'validated',co.convalidated,'deferrable',co.condeferrable,
              'deferred',co.condeferred,'parent',co.conparentid<>0,'local',co.conislocal,'ancestors',co.coninhcount)
            FROM pg_catalog.pg_constraint co JOIN pg_catalog.pg_class cl ON cl.oid=co.conrelid
            JOIN pg_catalog.pg_namespace n ON n.oid=cl.relnamespace
            LEFT JOIN pg_catalog.pg_attribute a ON a.attrelid=co.conrelid AND a.attnum=co.conkey[1]
            WHERE n.nspname=@schema AND co.contype='n'
            """,
        ["indexes"] = """
            SELECT jsonb_build_object('table',t.relname,'method',am.amname,'kind',i.relkind::text,'persistence',i.relpersistence::text,
              'primary',x.indisprimary,'unique',x.indisunique,'valid',x.indisvalid,'ready',x.indisready,'live',x.indislive,
              'immediate',x.indimmediate,'nullsNotDistinct',x.indnullsnotdistinct,'exclusion',x.indisexclusion,
              'keyCount',x.indnkeyatts,'totalCount',x.indnatts,
              'columns',ARRAY(SELECT a.attname FROM unnest(x.indkey) WITH ORDINALITY k(num,pos) LEFT JOIN pg_catalog.pg_attribute a ON a.attrelid=t.oid AND a.attnum=k.num ORDER BY k.pos),
              'collations',ARRAY(SELECT CASE WHEN co.oid IS NULL THEN NULL ELSE ns.nspname||'.'||co.collname END FROM unnest(x.indcollation) WITH ORDINALITY k(id,pos) LEFT JOIN pg_catalog.pg_collation co ON co.oid=k.id LEFT JOIN pg_catalog.pg_namespace ns ON ns.oid=co.collnamespace ORDER BY k.pos),
              'opclasses',ARRAY(SELECT ns.nspname||'.'||op.opcname FROM unnest(x.indclass) WITH ORDINALITY k(id,pos) JOIN pg_catalog.pg_opclass op ON op.oid=k.id JOIN pg_catalog.pg_namespace ns ON ns.oid=op.opcnamespace ORDER BY k.pos),
              'options',ARRAY(SELECT k.val::int FROM unnest(x.indoption) WITH ORDINALITY k(val,pos) ORDER BY k.pos),
              'expression',pg_catalog.pg_get_expr(x.indexprs,x.indrelid,false),'predicate',pg_catalog.pg_get_expr(x.indpred,x.indrelid,false))
            FROM pg_catalog.pg_index x JOIN pg_catalog.pg_class t ON t.oid=x.indrelid JOIN pg_catalog.pg_class i ON i.oid=x.indexrelid
            JOIN pg_catalog.pg_namespace n ON n.oid=t.relnamespace JOIN pg_catalog.pg_am am ON am.oid=i.relam WHERE n.nspname=@schema
            """,
        ["types"] = """
            SELECT jsonb_build_object('name',t.typname,'kind',t.typtype::text,'category',t.typcategory::text,
              'relation',c.relname,'element',e.typname,'elementSchema',en.nspname,
              'defined',t.typisdefined,'notNull',t.typnotnull,'default',t.typdefault)
            FROM pg_catalog.pg_type t JOIN pg_catalog.pg_namespace n ON n.oid=t.typnamespace
            LEFT JOIN pg_catalog.pg_class c ON c.oid=t.typrelid
            LEFT JOIN pg_catalog.pg_type e ON e.oid=t.typelem LEFT JOIN pg_catalog.pg_namespace en ON en.oid=e.typnamespace
            WHERE n.nspname=@schema
            """,
        ["sequences"] = """
            SELECT jsonb_build_object('name',s.relname,'kind',s.relkind::text,'persistence',s.relpersistence::text,
              'type',pg_catalog.format_type(q.seqtypid,NULL),'start',q.seqstart,'min',q.seqmin,'max',q.seqmax,
              'increment',q.seqincrement,'cache',q.seqcache,'cycle',q.seqcycle,'table',t.relname,
              'tableSchema',tn.nspname,'column',a.attname,'dependency',d.deptype::text,'usage',pg_catalog.has_sequence_privilege(s.oid,'USAGE'),
              'select',pg_catalog.has_sequence_privilege(s.oid,'SELECT'),'update',pg_catalog.has_sequence_privilege(s.oid,'UPDATE'),
              'ownerIsCurrent',s.relowner=(SELECT oid FROM pg_catalog.pg_roles WHERE rolname=current_user))
            FROM pg_catalog.pg_class s JOIN pg_catalog.pg_namespace n ON n.oid=s.relnamespace
            JOIN pg_catalog.pg_sequence q ON q.seqrelid=s.oid
            LEFT JOIN pg_catalog.pg_depend d ON d.classid='pg_catalog.pg_class'::regclass AND d.objid=s.oid AND d.refclassid='pg_catalog.pg_class'::regclass AND d.deptype IN ('a','i')
            LEFT JOIN pg_catalog.pg_class t ON t.oid=d.refobjid LEFT JOIN pg_catalog.pg_namespace tn ON tn.oid=t.relnamespace
            LEFT JOIN pg_catalog.pg_attribute a ON a.attrelid=t.oid AND a.attnum=d.refobjsubid WHERE n.nspname=@schema
            """,
        ["tablePrivileges"] = """
            SELECT jsonb_build_object('table',c.relname,'select',pg_catalog.has_table_privilege(c.oid,'SELECT'),
              'insert',pg_catalog.has_table_privilege(c.oid,'INSERT'),'update',pg_catalog.has_table_privilege(c.oid,'UPDATE'),
              'delete',pg_catalog.has_table_privilege(c.oid,'DELETE'),'truncate',pg_catalog.has_table_privilege(c.oid,'TRUNCATE'),
              'references',pg_catalog.has_table_privilege(c.oid,'REFERENCES'),'trigger',pg_catalog.has_table_privilege(c.oid,'TRIGGER'),
              'maintain',pg_catalog.has_table_privilege(c.oid,'MAINTAIN'))
            FROM pg_catalog.pg_class c JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace WHERE n.nspname=@schema AND c.relkind IN ('r','p','v','m','f')
            """,
        ["environment"] = """
            SELECT jsonb_build_object('connect',pg_catalog.has_database_privilege(current_database(),'CONNECT'),
              'databaseCreate',pg_catalog.has_database_privilege(current_database(),'CREATE'),'temporary',pg_catalog.has_database_privilege(current_database(),'TEMP'),
              'schemaUsage',pg_catalog.has_schema_privilege(@schema,'USAGE'),'schemaCreate',pg_catalog.has_schema_privilege(@schema,'CREATE'),
              'super',r.rolsuper,'createDb',r.rolcreatedb,'createRole',r.rolcreaterole,'bypassRls',r.rolbypassrls,'replication',r.rolreplication,'canLogin',r.rolcanlogin,
              'encoding',(SELECT pg_catalog.pg_encoding_to_char(encoding) FROM pg_catalog.pg_database WHERE datname=current_database()),
              'databaseOwner',r.oid=(SELECT datdba FROM pg_catalog.pg_database WHERE datname=current_database()),
              'schemaOwner',r.oid=(SELECT nspowner FROM pg_catalog.pg_namespace WHERE nspname=@schema))
            FROM pg_catalog.pg_roles r WHERE r.rolname=current_user
            """,
        ["unexpected"] = """
            SELECT jsonb_build_object('kind','trigger') FROM pg_catalog.pg_trigger t JOIN pg_catalog.pg_class c ON c.oid=t.tgrelid JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace WHERE n.nspname=@schema AND (NOT t.tgisinternal OR t.tgenabled<>'O' OR t.tgconstraint=0)
            UNION ALL SELECT jsonb_build_object('kind','rule') FROM pg_catalog.pg_rewrite r JOIN pg_catalog.pg_class c ON c.oid=r.ev_class JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','policy') FROM pg_catalog.pg_policy p JOIN pg_catalog.pg_class c ON c.oid=p.polrelid JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','routine') FROM pg_catalog.pg_proc p JOIN pg_catalog.pg_namespace n ON n.oid=p.pronamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','type') FROM pg_catalog.pg_type t JOIN pg_catalog.pg_namespace n ON n.oid=t.typnamespace WHERE n.nspname=@schema AND t.typrelid=0 AND NOT (t.typelem<>0 AND t.typtype='b')
            UNION ALL SELECT jsonb_build_object('kind','collation') FROM pg_catalog.pg_collation c JOIN pg_catalog.pg_namespace n ON n.oid=c.collnamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','extension') FROM pg_catalog.pg_extension e JOIN pg_catalog.pg_namespace n ON n.oid=e.extnamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','membership') FROM pg_catalog.pg_auth_members m JOIN pg_catalog.pg_roles r ON r.oid=m.member WHERE r.rolname=current_user
            UNION ALL SELECT jsonb_build_object('kind','inheritance') FROM pg_catalog.pg_inherits i JOIN pg_catalog.pg_class c ON c.oid=i.inhrelid JOIN pg_catalog.pg_namespace n ON n.oid=c.relnamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','operator') FROM pg_catalog.pg_operator o JOIN pg_catalog.pg_namespace n ON n.oid=o.oprnamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','opclass') FROM pg_catalog.pg_opclass o JOIN pg_catalog.pg_namespace n ON n.oid=o.opcnamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','opfamily') FROM pg_catalog.pg_opfamily o JOIN pg_catalog.pg_namespace n ON n.oid=o.opfnamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','statistics') FROM pg_catalog.pg_statistic_ext s JOIN pg_catalog.pg_namespace n ON n.oid=s.stxnamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','conversion') FROM pg_catalog.pg_conversion c JOIN pg_catalog.pg_namespace n ON n.oid=c.connamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','textSearch') FROM pg_catalog.pg_ts_config c JOIN pg_catalog.pg_namespace n ON n.oid=c.cfgnamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','textSearch') FROM pg_catalog.pg_ts_dict c JOIN pg_catalog.pg_namespace n ON n.oid=c.dictnamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','textSearch') FROM pg_catalog.pg_ts_parser c JOIN pg_catalog.pg_namespace n ON n.oid=c.prsnamespace WHERE n.nspname=@schema
            UNION ALL SELECT jsonb_build_object('kind','textSearch') FROM pg_catalog.pg_ts_template c JOIN pg_catalog.pg_namespace n ON n.oid=c.tmplnamespace WHERE n.nspname=@schema
            """
    };
}
