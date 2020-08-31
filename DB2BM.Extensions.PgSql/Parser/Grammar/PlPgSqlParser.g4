parser grammar PlPgSqlParser;

options {
    language=CSharp;
    tokenVocab=PlpgSqlLexer;
}

function_body
    : function_block SEMI_COLON? EOF;

function_block
    : start_label? declarations?
    BEGIN function_statements exception_statement?
    END? end_label=identifier?
    ;

id_token
  : Identifier | QuotedIdentifier | tokens_nonkeyword;

identifier
    : id_token
    | tokens_nonreserved
    | tokens_nonreserved_except_function_type
    ;

start_label
    : LESS_LESS identifier GREATER_GREATER
    ;

declarations
    : DECLARE declaration*
    ;

declaration
    : DECLARE* identifier type_declaration SEMI_COLON
    ;

type_declaration
    : CONSTANT? data_type_dec collate_identifier? (NOT NULL)? ((DEFAULT | COLON_EQUAL | EQUAL) vex)?
    | ALIAS FOR (identifier | DOLLAR_NUMBER)
    | (NO? SCROLL)? CURSOR (LEFT_PAREN arguments_list RIGHT_PAREN)? (FOR | IS) select_stmt
    ;

select_stmt
    : with_clause? select_ops after_ops*
    ;

select_ops
    : LEFT_PAREN select_stmt RIGHT_PAREN // parens can be used to apply "global" clauses (WITH etc) to a particular select in UNION expr
    | select_ops (INTERSECT | UNION | EXCEPT) set_qualifier? select_ops
    | select_primary
    ;

select_primary
    : SELECT
        (set_qualifier (ON LEFT_PAREN vex (COMMA vex)* RIGHT_PAREN)?)?
        select_list? into_table?
        (FROM from_item (COMMA from_item)*)?
        (WHERE vex)?
        groupby_clause?
        (HAVING vex)?
        (WINDOW identifier AS window_definition (COMMA identifier AS window_definition)*)?
    | TABLE ONLY? schema_qualified_name MULTIPLY?
    | values_stmt
    ;

window_definition
    : LEFT_PAREN identifier? partition_by_columns? orderby_clause? frame_clause? RIGHT_PAREN
    ;

frame_clause
    : (RANGE | ROWS | GROUPS) (frame_bound | BETWEEN frame_bound AND frame_bound)
    (EXCLUDE (CURRENT ROW | GROUP | TIES | NO OTHERS))?
    ;

frame_bound
    : vex (PRECEDING | FOLLOWING)
    | CURRENT ROW
    ;

orderby_clause
    : ORDER BY sort_specifier_list
    ;

sort_specifier_list
    : sort_specifier (COMMA sort_specifier)*
    ;

sort_specifier
    : key=vex
    opclass=schema_qualified_name? // this allows to share this rule with create_index; technically invalid syntax
    order=order_specification?
    null_order=null_ordering?
    ;

null_ordering
    : NULLS (FIRST | LAST)
    ;

order_specification
    : ASC | DESC | USING all_op_ref
    ;

all_op_ref
  : all_simple_op
  | OPERATOR LEFT_PAREN identifier DOT all_simple_op RIGHT_PAREN
  ;

all_simple_op
    : op_chars
    | EQUAL | NOT_EQUAL | LTH | LEQ | GTH | GEQ
    | PLUS | MINUS | MULTIPLY | DIVIDE | MODULAR | EXP
    ;

op_chars
    : OP_CHARS
    | LESS_LESS
    | GREATER_GREATER
    | HASH_SIGN
    ;

values_stmt
    : VALUES values_values (COMMA values_values)*
    ;

values_values
    : LEFT_PAREN (vex | DEFAULT) (COMMA (vex | DEFAULT))* RIGHT_PAREN
    ;

partition_by_columns
    : PARTITION BY vex (COMMA vex)*
    ;

schema_qualified_name
    : identifier ( DOT identifier ( DOT identifier )? )?
    ;

select_list
  : select_sublist (COMMA select_sublist)*
  ;

select_sublist
  : vex (AS col_label | id_token)?
  ;

col_label
    : id_token
    | tokens_reserved
    | tokens_nonreserved
    | tokens_nonreserved_except_function_type
    ;

groupby_clause
  : GROUP BY grouping_element_list
  ;

grouping_element_list
  : grouping_element (COMMA grouping_element)*
  ;

grouping_element
  : vex
  | LEFT_PAREN RIGHT_PAREN
  | (ROLLUP | CUBE | GROUPING SETS) LEFT_PAREN grouping_element_list RIGHT_PAREN
  ;

from_item
    : LEFT_PAREN from_item RIGHT_PAREN alias_clause?
    | from_item CROSS JOIN from_item
    | from_item (INNER | (LEFT | RIGHT | FULL) OUTER?)? JOIN from_item ON vex
    | from_item (INNER | (LEFT | RIGHT | FULL) OUTER?)? JOIN from_item USING names_in_parens
    | from_item NATURAL (INNER | (LEFT | RIGHT | FULL) OUTER?)? JOIN from_item
    | from_primary
    ;

from_primary
    : ONLY? schema_qualified_name MULTIPLY? alias_clause? (TABLESAMPLE method=identifier LEFT_PAREN vex (COMMA vex)* RIGHT_PAREN (REPEATABLE vex)?)?
    | LATERAL? table_subquery alias_clause
    | LATERAL? function_call (WITH ORDINALITY)?
        (AS from_function_column_def
        | AS? alias=identifier (LEFT_PAREN column_alias+=identifier (COMMA column_alias+=identifier)* RIGHT_PAREN | from_function_column_def)?
        )?
    | LATERAL? ROWS FROM LEFT_PAREN function_call (AS from_function_column_def)? (COMMA function_call (AS from_function_column_def)?)* RIGHT_PAREN
    (WITH ORDINALITY)? (AS? alias=identifier (LEFT_PAREN column_alias+=identifier (COMMA column_alias+=identifier)* RIGHT_PAREN)?)?
    ;

from_function_column_def
    : LEFT_PAREN column_alias+=identifier data_type (COMMA column_alias+=identifier data_type)* RIGHT_PAREN
    ;


data_type
    : SETOF? predefined_type (ARRAY array_type? | array_type+)?
    ;

array_type
    : LEFT_BRACKET NUMBER_LITERAL? RIGHT_BRACKET
    ;

predefined_type
    : BIGINT
    | BIT VARYING? type_length?
    | BOOLEAN
    | DEC precision_param?
    | DECIMAL precision_param?
    | DOUBLE PRECISION
    | FLOAT precision_param?
    | INT
    | INTEGER
    | INTERVAL interval_field? type_length?
    | NATIONAL? (CHARACTER | CHAR) VARYING? type_length?
    | NCHAR VARYING? type_length?
    | NUMERIC precision_param?
    | REAL
    | SMALLINT
    | TIME type_length? ((WITH | WITHOUT) TIME ZONE)?
    | TIMESTAMP type_length? ((WITH | WITHOUT) TIME ZONE)?
    | VARCHAR type_length?
    | schema_qualified_name_nontype (LEFT_PAREN vex (COMMA vex)* RIGHT_PAREN)?
    ;

schema_qualified_name_nontype
    : identifier_nontype
    | schema=identifier DOT identifier_nontype
    ;

identifier_nontype
    : id_token
    | tokens_nonreserved
    | tokens_reserved_except_function_type
    ;

interval_field
    : YEAR
    | MONTH
    | DAY
    | HOUR
    | MINUTE
    | SECOND
    | YEAR TO MONTH
    | DAY TO HOUR
    | DAY TO MINUTE
    | DAY TO SECOND
    | HOUR TO MINUTE
    | HOUR TO SECOND
    | MINUTE TO SECOND
    ;

type_length
    : LEFT_PAREN NUMBER_LITERAL RIGHT_PAREN
    ;

precision_param
    : LEFT_PAREN precision=NUMBER_LITERAL (COMMA scale=NUMBER_LITERAL)? RIGHT_PAREN
    ;    

function_call
    : schema_qualified_name_nontype LEFT_PAREN (set_qualifier? vex_or_named_notation (COMMA vex_or_named_notation)* orderby_clause?)? RIGHT_PAREN
        (WITHIN GROUP LEFT_PAREN orderby_clause RIGHT_PAREN)?
        filter_clause? (OVER (identifier | window_definition))?
    | function_construct
    | extract_function
    | system_function
    | date_time_function
    | string_value_function
    | xml_function
    ;

xml_function
    : XMLELEMENT LEFT_PAREN NAME name=identifier
        (COMMA XMLATTRIBUTES LEFT_PAREN vex (AS attname=identifier)? (COMMA vex (AS attname=identifier)?)* RIGHT_PAREN)?
        (COMMA vex)* RIGHT_PAREN
    | XMLFOREST LEFT_PAREN vex (AS name=identifier)? (COMMA vex (AS name=identifier)?)* RIGHT_PAREN
    | XMLPI LEFT_PAREN NAME name=identifier (COMMA vex)? RIGHT_PAREN
    | XMLROOT LEFT_PAREN vex COMMA VERSION (vex | NO VALUE) (COMMA STANDALONE (YES | NO | NO VALUE))? RIGHT_PAREN
    | XMLEXISTS LEFT_PAREN vex PASSING (BY REF)? vex (BY REF)? RIGHT_PAREN
    | XMLPARSE LEFT_PAREN (DOCUMENT | CONTENT) vex RIGHT_PAREN
    | XMLSERIALIZE LEFT_PAREN (DOCUMENT | CONTENT) vex AS data_type RIGHT_PAREN
    | XMLTABLE LEFT_PAREN 
        (XMLNAMESPACES LEFT_PAREN vex AS name=identifier (COMMA vex AS name=identifier)* RIGHT_PAREN COMMA)?
        vex PASSING (BY REF)? vex (BY REF)?
        COLUMNS xml_table_column (COMMA xml_table_column)*
        RIGHT_PAREN
    ;

xml_table_column
    : name=identifier (data_type (PATH vex)? (DEFAULT vex)? (NOT? NULL)? | FOR ORDINALITY)
    ;

string_value_function
    : TRIM LEFT_PAREN (LEADING | TRAILING | BOTH)? (chars=vex FROM str=vex | FROM? str=vex (COMMA chars=vex)?) RIGHT_PAREN
    | SUBSTRING LEFT_PAREN vex (COMMA vex)* (FROM vex)? (FOR vex)? RIGHT_PAREN
    | POSITION LEFT_PAREN vex_b IN vex RIGHT_PAREN
    | OVERLAY LEFT_PAREN vex PLACING vex FROM vex (FOR vex)? RIGHT_PAREN
    | COLLATION FOR LEFT_PAREN vex RIGHT_PAREN
    ;

date_time_function
    : CURRENT_DATE
    | CURRENT_TIME type_length?
    | CURRENT_TIMESTAMP type_length?
    | LOCALTIME type_length?
    | LOCALTIMESTAMP type_length?
    ;

system_function
    : CURRENT_CATALOG
    // parens are handled by generic function call
    // since current_schema is defined as reserved(can be function) keyword
    | CURRENT_SCHEMA /*(LEFT_PAREN RIGHT_PAREN)?*/
    | CURRENT_USER
    | SESSION_USER
    | USER
    | cast_specification
    ;

cast_specification
  : (CAST | TREAT) LEFT_PAREN vex AS data_type RIGHT_PAREN
  ;

extract_function
    : EXTRACT LEFT_PAREN (identifier | character_string) FROM vex RIGHT_PAREN
    ;

character_string
    : BeginDollarStringConstant Text_between_Dollar* EndDollarStringConstant
    | Character_String_Literal
    ;

function_construct
    : (COALESCE | GREATEST | GROUPING | LEAST | NULLIF | XMLCONCAT) LEFT_PAREN vex (COMMA vex)* RIGHT_PAREN
    | ROW LEFT_PAREN (vex (COMMA vex)*)? RIGHT_PAREN
    ; 

filter_clause
    : FILTER LEFT_PAREN WHERE vex RIGHT_PAREN
    ;

vex_or_named_notation
    : VARIADIC? (argname=identifier pointer)? vex
    ;

pointer
    : EQUAL_GTH | COLON_EQUAL
    ;

table_subquery
    : LEFT_PAREN select_stmt RIGHT_PAREN
    ;

names_in_parens
    : LEFT_PAREN names_references RIGHT_PAREN
    ;

names_references
    : schema_qualified_name (COMMA schema_qualified_name)*
    ;  
alias_clause
    : AS? alias=identifier (LEFT_PAREN column_alias+=identifier (COMMA column_alias+=identifier)* RIGHT_PAREN)?
    ;

into_table
    : INTO (TEMPORARY | TEMP | UNLOGGED)? TABLE? schema_qualified_name (COMMA schema_qualified_name)*
    ;

set_qualifier
    : DISTINCT | ALL
    ;

with_clause
    : WITH RECURSIVE? with_query (COMMA with_query)*
    ;

with_query
    : query_name=identifier (LEFT_PAREN column_name+=identifier (COMMA column_name+=identifier)* RIGHT_PAREN)?
    AS (NOT? MATERIALIZED)? LEFT_PAREN (select_stmt | insert_stmt_for_psql | update_stmt_for_psql | delete_stmt_for_psql) RIGHT_PAREN
    ;

delete_stmt_for_psql
    : with_clause? DELETE FROM ONLY? delete_table_name=schema_qualified_name MULTIPLY? (AS? alias=identifier)?
    (USING from_item (COMMA from_item)*)?
    (WHERE (vex | CURRENT OF cursor=identifier))?
    (RETURNING select_list)?
    ;

update_stmt_for_psql
    : with_clause? UPDATE ONLY? update_table_name=schema_qualified_name MULTIPLY? (AS? alias=identifier)?
    SET update_set (COMMA update_set)*
    (FROM from_item (COMMA from_item)*)?
    (WHERE (vex | CURRENT OF cursor=identifier))?
    (RETURNING select_list)?
    ;

update_set
    : column+=indirection_identifier EQUAL (value+=vex | DEFAULT)
    | LEFT_PAREN column+=indirection_identifier (COMMA column+=indirection_identifier)* RIGHT_PAREN EQUAL ROW?
    (LEFT_PAREN (value+=vex | DEFAULT) (COMMA (value+=vex | DEFAULT))* RIGHT_PAREN | table_subquery)
    ;

indirection_identifier
    : identifier indirection_list?
    ;

indirection_list
    : indirection+ 
    | indirection* DOT MULTIPLY
    ;

indirection
    : DOT col_label
    | LEFT_BRACKET vex RIGHT_BRACKET
    | LEFT_BRACKET vex? COLON vex? RIGHT_BRACKET
    ;

insert_stmt_for_psql
    : with_clause? INSERT INTO insert_table_name=schema_qualified_name (AS alias=identifier)?
    (OVERRIDING (SYSTEM | USER) VALUE)? insert_columns?
    (select_stmt | DEFAULT VALUES)
    (ON CONFLICT conflict_object? conflict_action)?
    (RETURNING select_list)?
    ;

conflict_action
    : DO NOTHING
    | DO UPDATE SET update_set (COMMA update_set)* (WHERE vex)?
    ;

conflict_object
    : index_sort index_where?
    | ON CONSTRAINT identifier
    ;

index_where
    : WHERE vex
    ;

index_sort
    : LEFT_PAREN sort_specifier_list RIGHT_PAREN
    ;

insert_columns
    : LEFT_PAREN indirection_identifier (COMMA indirection_identifier)* RIGHT_PAREN
    ;

after_ops
    : orderby_clause
    | LIMIT (vex | ALL)
    | OFFSET vex (ROW | ROWS)?
    | FETCH (FIRST | NEXT) vex? (ROW | ROWS) ONLY?
    | FOR (UPDATE | NO KEY UPDATE | SHARE | KEY SHARE) (OF schema_qualified_name (COMMA schema_qualified_name)*)? (NOWAIT | SKIP_ LOCKED)?
    ;

arguments_list
    : identifier data_type (COMMA identifier data_type)*
    ;

collate_identifier
    : COLLATE collation=schema_qualified_name
    ;

data_type_dec
    : data_type
    | schema_qualified_name MODULAR TYPE
    | schema_qualified_name_nontype MODULAR ROWTYPE
    ;

exception_statement
    : EXCEPTION (WHEN vex THEN function_statements)+
    ;

function_statements
    : (function_statement SEMI_COLON?)*
    ;

function_statement
    : function_block
    | base_statement
    | control_statement
    | transaction_statement
    | cursor_statement
    | message_statement
    | data_statement
    | additional_statement
    ;

base_statement
    : assign_stmt
    | EXECUTE vex using_vex?
    | PERFORM perform_stmt
    | GET (CURRENT | STACKED)? DIAGNOSTICS diagnostic_option (COMMA diagnostic_option)*
    | NULL
    ;

diagnostic_option
    : var (COLON_EQUAL | EQUAL) identifier
    ;

var
    : (schema_qualified_name | DOLLAR_NUMBER) (LEFT_BRACKET vex RIGHT_BRACKET)*
    ;
    
perform_stmt
    : (set_qualifier (ON LEFT_PAREN vex (COMMA vex)* RIGHT_PAREN)?)?
    select_list
    (FROM from_item (COMMA from_item)*)?
    (WHERE vex)?
    groupby_clause?
    (HAVING vex)?
    (WINDOW identifier AS window_definition (COMMA identifier AS window_definition)*)?
    ((INTERSECT | UNION | EXCEPT) set_qualifier? select_ops)?
    after_ops*
    ;

control_statement
    : return_stmt
    | CALL function_call
    | if_statement
    | case_statement
    | loop_statement
    ;

return_stmt
    : RETURN QUERY (select_stmt | execute_stmt | show_statement | explain_statement)
    | RETURN NEXT vex
    | RETURN perform_stmt?
    ;

explain_statement
    : EXPLAIN (ANALYZE? VERBOSE? | LEFT_PAREN explain_option (COMMA explain_option)* RIGHT_PAREN) explain_query;

explain_query
    : data_statement
    | values_stmt
    | execute_statement
    | declare_statement
    | CREATE (create_table_as_statement | create_view_statement)
    ;

create_table_as_statement
    : ((GLOBAL | LOCAL)? (TEMPORARY | TEMP) | UNLOGGED)? TABLE if_not_exists? name=schema_qualified_name
    names_in_parens?
    (USING identifier)?
    storage_parameter_oid?
    on_commit?
    table_space?
    AS (select_stmt | EXECUTE function_call)
    (WITH NO? DATA)?
    ;

storage_parameter_oid
    : with_storage_parameter | (WITH OIDS) | (WITHOUT OIDS)
    ;

with_storage_parameter
    : WITH storage_parameter
    ;

storage_parameter
    : LEFT_PAREN storage_parameter_option (COMMA storage_parameter_option)* RIGHT_PAREN
    ;

storage_parameter_option
    : schema_qualified_name (EQUAL vex)?
    ;

on_commit
    : ON COMMIT (PRESERVE ROWS | DELETE ROWS | DROP)
    ;

table_space
    : TABLESPACE identifier
    ;
if_not_exists
    : IF NOT EXISTS
    ;

create_view_statement
    : (OR REPLACE)? (TEMP | TEMPORARY)? RECURSIVE? MATERIALIZED? VIEW 
    if_not_exists? name=schema_qualified_name column_names=view_columns?
    (USING identifier)?
    (WITH storage_parameter)?
    table_space?
    AS v_query=select_stmt
    with_check_option?
    (WITH NO? DATA)?
    ;

view_columns
    : LEFT_PAREN identifier (COMMA identifier)* RIGHT_PAREN
    ;

with_check_option
    : WITH (CASCADED|LOCAL)? CHECK OPTION
    ;

execute_statement
    : EXECUTE identifier (LEFT_PAREN vex (COMMA vex)* RIGHT_PAREN)?
    ;

declare_statement
    : DECLARE identifier BINARY? INSENSITIVE? (NO? SCROLL)? CURSOR ((WITH | WITHOUT) HOLD)? FOR select_stmt
    ;

explain_option
    : (ANALYZE | VERBOSE | COSTS | SETTINGS | BUFFERS | TIMING | SUMMARY) boolean_value?
    | FORMAT (TEXT | XML | JSON | YAML)
    ;

boolean_value
    : TRUE 
    | FALSE 
    | OFF 
    | ON 
    | NUMBER_LITERAL
    ;

show_statement
    : SHOW ((identifier DOT)? identifier | ALL | TIME ZONE | TRANSACTION ISOLATION LEVEL | SESSION AUTHORIZATION)
    ;

execute_stmt
    : EXECUTE vex using_vex?
    ;

if_statement
    : IF vex THEN function_statements ((ELSIF | ELSEIF) vex THEN function_statements)* (ELSE function_statements)? END IF
    ;

case_statement
    : CASE vex? (WHEN vex (COMMA vex)* THEN function_statements)+ (ELSE function_statements)? END CASE
    ;

loop_statement
    : start_label? loop_start? LOOP function_statements END LOOP identifier?
    | (EXIT | CONTINUE) identifier? (WHEN vex)?
    ;

loop_start
    : WHILE vex
    | FOR alias=identifier IN REVERSE? vex DOUBLE_DOT vex (BY vex)?
    | FOR identifier_list IN (select_stmt | execute_stmt)
    | FOR cursor=identifier IN identifier (LEFT_PAREN option (COMMA option)* RIGHT_PAREN)? // cursor loop
    | FOREACH identifier_list (SLICE NUMBER_LITERAL)? IN ARRAY vex
    ;

option
    : (identifier COLON_EQUAL)? vex
    ;

identifier_list
    : identifier (COMMA identifier)*
    ;

transaction_statement
    : (COMMIT | ROLLBACK) (AND NO? CHAIN)?
    | lock_table
    ;   

lock_table
    : LOCK TABLE? only_table_multiply (COMMA only_table_multiply)* (IN lock_mode MODE)? NOWAIT?
    ;

lock_mode
    : (ROW | ACCESS) SHARE
    | ROW EXCLUSIVE
    | SHARE (ROW | UPDATE) EXCLUSIVE
    | SHARE
    | ACCESS? EXCLUSIVE
    ;

only_table_multiply
    : ONLY? schema_qualified_name MULTIPLY?
    ;

cursor_statement
    : OPEN var (NO? SCROLL)? FOR (select_stmt | execute_stmt)
    | OPEN var (LEFT_PAREN option (COMMA option)* RIGHT_PAREN)?
    | FETCH fetch_move_direction? (FROM | IN)? var into_table?
    | MOVE fetch_move_direction? (FROM | IN)? var
    | CLOSE var
    ;

fetch_move_direction
    : NEXT
    | PRIOR
    | FIRST
    | LAST
    | (ABSOLUTE | RELATIVE)? signed_number_literal
    | ALL
    | FORWARD (NUMBER_LITERAL | ALL)?
    | BACKWARD (NUMBER_LITERAL | ALL)?
    ;

signed_number_literal
    : sign? NUMBER_LITERAL
    ;

sign
    : PLUS | MINUS
    ;

message_statement
    : RAISE log_level? (character_string (COMMA vex)*)? raise_using?
    | RAISE log_level? identifier raise_using?
    | RAISE log_level? SQLSTATE character_string raise_using?
    | ASSERT vex (COMMA vex)?
    ;

raise_using
    : USING raise_param EQUAL vex (COMMA raise_param EQUAL vex)*
    ;

raise_param
    : MESSAGE
    | DETAIL
    | HINT
    | ERRCODE
    | COLUMN
    | CONSTRAINT
    | DATATYPE
    | TABLE
    | SCHEMA
    ;

log_level
    : DEBUG
    | LOG
    | INFO
    | NOTICE
    | WARNING
    | EXCEPTION
    ;

data_statement
    : select_stmt
    | insert_stmt_for_psql
    | update_stmt_for_psql
    | delete_stmt_for_psql
    | notify_stmt
    | truncate_stmt
    ;

notify_stmt
    : NOTIFY channel=identifier (COMMA payload=character_string)?
    ;

truncate_stmt
    : TRUNCATE TABLE? only_table_multiply (COMMA only_table_multiply)*
    ((RESTART | CONTINUE) IDENTITY)? cascade_restrict?
    ;

cascade_restrict
    : CASCADE | RESTRICT
    ;

additional_statement
    : anonymous_block
    | LISTEN identifier
    | UNLISTEN (identifier | MULTIPLY)
    | ANALYZE (LEFT_PAREN analyze_mode (COMMA analyze_mode)* RIGHT_PAREN | VERBOSE)? table_cols_list?
    | CLUSTER VERBOSE? (identifier ON schema_qualified_name | schema_qualified_name (USING identifier)?)?
    | CHECKPOINT
    | LOAD Character_String_Literal
    | DEALLOCATE PREPARE? (identifier | ALL)
    | REINDEX (LEFT_PAREN VERBOSE RIGHT_PAREN)? (INDEX | TABLE | SCHEMA | DATABASE | SYSTEM) CONCURRENTLY? schema_qualified_name
    | RESET ((identifier DOT)? identifier | TIME ZONE | SESSION AUTHORIZATION | ALL)
    | explain_statement
    | REFRESH MATERIALIZED VIEW CONCURRENTLY? schema_qualified_name (WITH NO? DATA)?
    | PREPARE identifier (LEFT_PAREN data_type (COMMA data_type)* RIGHT_PAREN)? AS data_statement
    | REASSIGN OWNED BY user_name (COMMA user_name)* TO user_name
    | copy_statement
    | show_statement
    ;

copy_statement
    : copy_to_statement 
    | copy_from_statement
    ;

copy_from_statement
    : COPY table_cols
    FROM (PROGRAM? Character_String_Literal | STDIN) 
    (WITH? (LEFT_PAREN copy_option_list RIGHT_PAREN | copy_option_list))?
    (WHERE vex)?
    ;

copy_to_statement
    : COPY (table_cols | LEFT_PAREN (select_stmt | insert_stmt_for_psql | update_stmt_for_psql | delete_stmt_for_psql) RIGHT_PAREN)
    TO (PROGRAM? Character_String_Literal | STDOUT)
    (WITH? (LEFT_PAREN copy_option_list RIGHT_PAREN | copy_option_list))?
    ;

user_name
    : identifier | CURRENT_USER | SESSION_USER
    ;
copy_option_list
    : copy_option (COMMA? copy_option)*
    ;

copy_option
    : FORMAT? (TEXT | CSV | BINARY)
    | OIDS truth_value?
    | FREEZE truth_value?
    | DELIMITER AS? Character_String_Literal
    | NULL AS? Character_String_Literal
    | HEADER truth_value?
    | QUOTE Character_String_Literal
    | ESCAPE Character_String_Literal
    | FORCE QUOTE (MULTIPLY | identifier_list)
    | FORCE_QUOTE (MULTIPLY | LEFT_PAREN identifier_list RIGHT_PAREN)
    | FORCE NOT NULL identifier_list
    | FORCE_NOT_NULL LEFT_PAREN identifier_list RIGHT_PAREN
    | FORCE_NULL LEFT_PAREN identifier_list RIGHT_PAREN
    | ENCODING Character_String_Literal
    ;

truth_value
  : TRUE | FALSE | ON // on is reserved but is required by SET statements
  ;

table_cols_list
    : table_cols (COMMA table_cols)*
    ;

table_cols
    : schema_qualified_name (LEFT_PAREN identifier (COMMA identifier)* RIGHT_PAREN)?
    ;

anonymous_block
    : DO (LANGUAGE (identifier | character_string))? character_string
    | DO character_string LANGUAGE (identifier | character_string)
    ;

analyze_mode
    : (VERBOSE | SKIP_LOCKED) boolean_value?
    ;

using_vex
    : USING vex (COMMA vex)*
    ;

assign_stmt
    : var (COLON_EQUAL | EQUAL) (select_stmt_no_parens | perform_stmt)
    ;   

select_stmt_no_parens
    : with_clause? select_ops_no_parens after_ops*
    ;

select_ops_no_parens
    : select_ops (INTERSECT | UNION | EXCEPT) set_qualifier? (select_primary | LEFT_PAREN select_stmt RIGHT_PAREN)
    | select_primary
    ;

vex_b
  : vex_b CAST_EXPRESSION data_type
  | LEFT_PAREN vex RIGHT_PAREN indirection_list?
  | LEFT_PAREN vex (COMMA vex)+ RIGHT_PAREN
  | <assoc=right> (PLUS | MINUS) vex_b
  | vex_b EXP vex_b
  | vex_b (MULTIPLY | DIVIDE | MODULAR) vex_b
  | vex_b (PLUS | MINUS) vex_b
  | vex_b op vex_b
  | op vex_b
  | vex_b op
  | vex_b (LTH | GTH | LEQ | GEQ | EQUAL | NOT_EQUAL) vex_b
  | vex_b IS NOT? DISTINCT FROM vex_b
  | vex_b IS NOT? DOCUMENT
  | vex_b IS NOT? UNKNOWN
  | vex_b IS NOT? OF LEFT_PAREN type_list RIGHT_PAREN
  | value_expression_primary
  ;

op
  : op_chars
  | OPERATOR LEFT_PAREN identifier DOT all_simple_op RIGHT_PAREN
  ;

value_expression_primary
  : unsigned_value_specification
  | LEFT_PAREN select_stmt_no_parens RIGHT_PAREN indirection_list?
  | case_expression
  | NULL
  | MULTIPLY
  // technically incorrect since ANY cannot be value expression
  // but fixing this would require to write a vex rule duplicating all operators
  // like vex (op|op|op|...) comparison_mod
  | comparison_mod
  | EXISTS table_subquery
  | function_call
  | indirection_var
  | array_expression
  | type_coercion
  | datetime_overlaps
  ;

datetime_overlaps
  : LEFT_PAREN vex COMMA vex RIGHT_PAREN OVERLAPS LEFT_PAREN vex COMMA vex RIGHT_PAREN
  ;

type_coercion
    : data_type character_string
    | INTERVAL character_string interval_field type_length?
    ;

array_expression
    : ARRAY (array_elements | table_subquery)
    ;

array_elements
    : LEFT_BRACKET ((vex | array_elements) (COMMA (vex | array_elements))*)? RIGHT_BRACKET
    ;

indirection_var
    : (identifier | dollar_number) indirection_list?
    ;

dollar_number
    : DOLLAR_NUMBER
    ;
    
comparison_mod
    : (ALL | ANY | SOME) LEFT_PAREN (vex | select_stmt_no_parens) RIGHT_PAREN
    ;

case_expression
  : CASE vex? (WHEN vex THEN r+=vex)+ (ELSE r+=vex)? END
  ;

unsigned_value_specification
  : unsigned_numeric_literal
  | character_string
  | truth_value
  ;

unsigned_numeric_literal
  : NUMBER_LITERAL
  | REAL_NUMBER
  ;
type_list
    : data_type (COMMA data_type)*
    ;

vex
  : vex CAST_EXPRESSION data_type
  | LEFT_PAREN vex RIGHT_PAREN indirection_list?
  | LEFT_PAREN vex (COMMA vex)+ RIGHT_PAREN
  | vex collate_identifier
  | <assoc=right> (PLUS | MINUS) vex
  | vex AT TIME ZONE vex
  | vex EXP vex
  | vex (MULTIPLY | DIVIDE | MODULAR) vex
  | vex (PLUS | MINUS) vex
  // TODO a lot of ambiguities between 3 next alternatives
  | vex op vex
  | op vex
  | vex op
  | vex NOT? IN LEFT_PAREN (select_stmt_no_parens | vex (COMMA vex)*) RIGHT_PAREN
  | vex NOT? BETWEEN (ASYMMETRIC | SYMMETRIC)? vex_b AND vex
  | vex NOT? (LIKE | ILIKE | SIMILAR TO) vex
  | vex NOT? (LIKE | ILIKE | SIMILAR TO) vex ESCAPE vex
  | vex (LTH | GTH | LEQ | GEQ | EQUAL | NOT_EQUAL) vex
  | vex IS NOT? (truth_value | NULL)
  | vex IS NOT? DISTINCT FROM vex
  | vex IS NOT? DOCUMENT
  | vex IS NOT? UNKNOWN
  | vex IS NOT? OF LEFT_PAREN type_list RIGHT_PAREN
  | vex ISNULL
  | vex NOTNULL
  | <assoc=right> NOT vex
  | vex AND vex
  | vex OR vex
  | value_expression_primary
  ;

tokens_nonreserved
    : ABORT
    | ABSOLUTE
    | ACCESS
    | ACTION
    | ADD
    | ADMIN
    | AFTER
    | AGGREGATE
    | ALSO
    | ALTER
    | ALWAYS
    | ASSERTION
    | ASSIGNMENT
    | AT
    | ATTACH
    | ATTRIBUTE
    | BACKWARD
    | BEFORE
    | BEGIN
    | BY
    | CACHE
    | CALL
    | CALLED
    | CASCADE
    | CASCADED
    | CATALOG
    | CHAIN
    | CHARACTERISTICS
    | CHECKPOINT
    | CLASS
    | CLOSE
    | CLUSTER
    | COLUMNS
    | COMMENT
    | COMMENTS
    | COMMIT
    | COMMITTED
    | CONFIGURATION
    | CONFLICT
    | CONNECTION
    | CONSTRAINTS
    | CONTENT
    | CONTINUE
    | CONVERSION
    | COPY
    | COST
    | CSV
    | CUBE
    | CURRENT
    | CURSOR
    | CYCLE
    | DATA
    | DATABASE
    | DAY
    | DEALLOCATE
    | DECLARE
    | DEFAULTS
    | DEFERRED
    | DEFINER
    | DELETE
    | DELIMITER
    | DELIMITERS
    | DEPENDS
    | DETACH
    | DICTIONARY
    | DISABLE
    | DISCARD
    | DOCUMENT
    | DOMAIN
    | DOUBLE
    | DROP
    | EACH
    | ENABLE
    | ENCODING
    | ENCRYPTED
    | ENUM
    | ESCAPE
    | EVENT
    | EXCLUDE
    | EXCLUDING
    | EXCLUSIVE
    | EXECUTE
    | EXPLAIN
    | EXTENSION
    | EXTERNAL
    | FAMILY
    | FILTER
    | FIRST
    | FOLLOWING
    | FORCE
    | FORWARD
    | FUNCTION
    | FUNCTIONS
    | GENERATED
    | GLOBAL
    | GRANTED
    | GROUPS
    | HANDLER
    | HEADER
    | HOLD
    | HOUR
    | IDENTITY
    | IF
    | IMMEDIATE
    | IMMUTABLE
    | IMPLICIT
    | IMPORT
    | INCLUDE
    | INCLUDING
    | INCREMENT
    | INDEX
    | INDEXES
    | INHERIT
    | INHERITS
    | INLINE
    | INPUT
    | INSENSITIVE
    | INSERT
    | INSTEAD
    | INVOKER
    | ISOLATION
    | KEY
    | LABEL
    | LANGUAGE
    | LARGE
    | LAST
    | LEAKPROOF
    | LEVEL
    | LISTEN
    | LOAD
    | LOCAL
    | LOCATION
    | LOCK
    | LOCKED
    | LOGGED
    | MAPPING
    | MATCH
    | MATERIALIZED
    | MAXVALUE
    | METHOD
    | MINUTE
    | MINVALUE
    | MODE
    | MONTH
    | MOVE
    | NAME
    | NAMES
    | NEW
    | NEXT
    | NO
    | NOTHING
    | NOTIFY
    | NOWAIT
    | NULLS
    | OBJECT
    | OF
    | OFF
    | OIDS
    | OLD
    | OPERATOR
    | OPTION
    | OPTIONS
    | ORDINALITY
    | OTHERS
    | OVER
    | OVERRIDING
    | OWNED
    | OWNER
    | PARALLEL
    | PARSER
    | PARTIAL
    | PARTITION
    | PASSING
    | PASSWORD
    | PLANS
    | POLICY
    | PRECEDING
    | PREPARE
    | PREPARED
    | PRESERVE
    | PRIOR
    | PRIVILEGES
    | PROCEDURAL
    | PROCEDURE
    | PROCEDURES
    | PROGRAM
    | PUBLICATION
    | QUOTE
    | RANGE
    | READ
    | REASSIGN
    | RECHECK
    | RECURSIVE
    | REF
    | REFERENCING
    | REFRESH
    | REINDEX
    | RELATIVE
    | RELEASE
    | RENAME
    | REPEATABLE
    | REPLACE
    | REPLICA
    | RESET
    | RESTART
    | RESTRICT
    | RETURNS
    | REVOKE
    | ROLE
    | ROLLBACK
    | ROLLUP
    | ROUTINE
    | ROUTINES
    | ROWS
    | RULE
    | SAVEPOINT
    | SCHEMA
    | SCHEMAS
    | SCROLL
    | SEARCH
    | SECOND
    | SECURITY
    | SEQUENCE
    | SEQUENCES
    | SERIALIZABLE
    | SERVER
    | SESSION
    | SET
    | SETS
    | SHARE
    | SHOW
    | SIMPLE
    | SKIP_
    | SNAPSHOT
    | SQL
    | STABLE
    | STANDALONE
    | START
    | STATEMENT
    | STATISTICS
    | STDIN
    | STDOUT
    | STORAGE
    | STORED
    | STRICT
    | STRIP
    | SUBSCRIPTION
    | SUPPORT
    | SYSID
    | SYSTEM
    | TABLES
    | TABLESPACE
    | TEMP
    | TEMPLATE
    | TEMPORARY
    | TEXT
    | TIES
    | TRANSACTION
    | TRANSFORM
    | TRIGGER
    | TRUNCATE
    | TRUSTED
    | TYPE
    | TYPES
    | UNBOUNDED
    | UNCOMMITTED
    | UNENCRYPTED
    | UNKNOWN
    | UNLISTEN
    | UNLOGGED
    | UNTIL
    | UPDATE
    | VACUUM
    | VALID
    | VALIDATE
    | VALIDATOR
    | VALUE
    | VARYING
    | VERSION
    | VIEW
    | VIEWS
    | VOLATILE
    | WHITESPACE
    | WITHIN
    | WITHOUT
    | WORK
    | WRAPPER
    | WRITE
    | XML
    | YEAR
    | YES
    | ZONE
    ;

tokens_nonreserved_except_function_type
    : BETWEEN
    | BIGINT
    | BIT
    | BOOLEAN
    | CHAR
    | CHARACTER
    | COALESCE
    | DEC
    | DECIMAL
    | EXISTS
    | EXTRACT
    | FLOAT
    | GREATEST
    | GROUPING
    | INOUT
    | INT
    | INTEGER
    | INTERVAL
    | LEAST
    | NATIONAL
    | NCHAR
    | NONE
    | NULLIF
    | NUMERIC
    | OUT
    | OVERLAY
    | POSITION
    | PRECISION
    | REAL
    | ROW
    | SETOF
    | SMALLINT
    | SUBSTRING
    | TIME
    | TIMESTAMP
    | TREAT
    | TRIM
    | VALUES
    | VARCHAR
    | XMLATTRIBUTES
    | XMLCONCAT
    | XMLELEMENT
    | XMLEXISTS
    | XMLFOREST
    | XMLNAMESPACES
    | XMLPARSE
    | XMLPI
    | XMLROOT
    | XMLSERIALIZE
    | XMLTABLE
    ;

tokens_reserved_except_function_type
    : AUTHORIZATION
    | BINARY
    | COLLATION
    | CONCURRENTLY
    | CROSS
    | CURRENT_SCHEMA
    | FREEZE
    | FULL
    | ILIKE
    | INNER
    | IS
    | ISNULL
    | JOIN
    | LEFT
    | LIKE
    | NATURAL
    | NOTNULL
    | OUTER
    | OVERLAPS
    | RIGHT
    | SIMILAR
    | TABLESAMPLE
    | VERBOSE
    ;

tokens_reserved
    : ALL
    | ANALYSE
    | ANALYZE
    | AND
    | ANY
    | ARRAY
    | AS
    | ASC
    | ASYMMETRIC
    | BOTH
    | CASE
    | CAST
    | CHECK
    | COLLATE
    | COLUMN
    | CONSTRAINT
    | CREATE
    | CURRENT_CATALOG
    | CURRENT_DATE
    | CURRENT_ROLE
    | CURRENT_TIME
    | CURRENT_TIMESTAMP
    | CURRENT_USER
    | DEFAULT
    | DEFERRABLE
    | DESC
    | DISTINCT
    | DO
    | ELSE
    | END
    | EXCEPT
    | FALSE
    | FETCH
    | FOR
    | FOREIGN
    | FROM
    | GRANT
    | GROUP
    | HAVING
    | IN
    | INITIALLY
    | INTERSECT
    | INTO
    | LATERAL
    | LEADING
    | LIMIT
    | LOCALTIME
    | LOCALTIMESTAMP
    | NOT
    | NULL
    | OFFSET
    | ON
    | ONLY
    | OR
    | ORDER
    | PLACING
    | PRIMARY
    | REFERENCES
    | RETURNING
    | SELECT
    | SESSION_USER
    | SOME
    | SYMMETRIC
    | TABLE
    | THEN
    | TO
    | TRAILING
    | TRUE
    | UNION
    | UNIQUE
    | USER
    | USING
    | VARIADIC
    | WHEN
    | WHERE
    | WINDOW
    | WITH
    ;

tokens_nonkeyword
    : ALIGNMENT
    | BASETYPE
    | BUFFERS
    | BYPASSRLS
    | CANONICAL
    | CATEGORY
    | COLLATABLE
    | COMBINEFUNC
    | COMMUTATOR
    | CONNECT
    | COSTS
    | CREATEDB
    | CREATEROLE
    | DESERIALFUNC
    | DETERMINISTIC
    | DISABLE_PAGE_SKIPPING
    | ELEMENT
    | EXTENDED
    | FINALFUNC
    | FINALFUNC_EXTRA
    | FINALFUNC_MODIFY
    | FORCE_NOT_NULL
    | FORCE_NULL
    | FORCE_QUOTE
    | FORMAT
    | GETTOKEN
    | HASH
    | HASHES
    | HEADLINE
    | HYPOTHETICAL
    | INDEX_CLEANUP
    | INIT
    | INITCOND
    | INTERNALLENGTH
    | JSON
    | LC_COLLATE
    | LC_CTYPE 
    | LEFTARG
    | LEXIZE
    | LEXTYPES
    | LIST
    | LOCALE 
    | LOGIN
    | MAIN
    | MERGES
    | MFINALFUNC
    | MFINALFUNC_EXTRA
    | MFINALFUNC_MODIFY
    | MINITCOND
    | MINVFUNC
    | MODULUS
    | MSFUNC
    | MSSPACE
    | MSTYPE
    | NEGATOR
    | NOBYPASSRLS
    | NOCREATEDB
    | NOCREATEROLE
    | NOINHERIT
    | NOLOGIN
    | NOREPLICATION
    | NOSUPERUSER
    | OUTPUT
    | PASSEDBYVALUE
    | PATH
    | PERMISSIVE
    | PLAIN
    | PREFERRED
    | PROVIDER
    | READ_ONLY
    | READ_WRITE
    | RECEIVE
    | REPLICATION
    | REMAINDER
    | RESTRICTED
    | RESTRICTIVE
    | RIGHTARG
    | SAFE
    | SEND
    | SERIALFUNC
    | SETTINGS
    | SFUNC
    | SHAREABLE
    | SKIP_LOCKED
    | SORTOP
    | SSPACE
    | STYPE
    | SUBTYPE
    | SUBTYPE_DIFF
    | SUBTYPE_OPCLASS
    | SUMMARY
    | SUPERUSER
    | TIMING
    | TYPMOD_IN
    | TYPMOD_OUT
    | UNSAFE
    | USAGE
    | VARIABLE
    | YAML

    // plpgsql tokens
    | ALIAS
    | ASSERT
    | CONSTANT
    | DATATYPE
    | DEBUG
    | DETAIL
    | DIAGNOSTICS
    | ELSEIF
    | ELSIF
    | ERRCODE
    | EXIT
    | EXCEPTION
    | FOREACH
    | GET
    | HINT
    | INFO
    | LOG
    | LOOP
    | MESSAGE
    | NOTICE
    | OPEN
    | PERFORM
    | QUERY
    | RAISE
    | RECORD
    | RETURN
    | REVERSE
    | ROWTYPE
    | SLICE
    | SQLSTATE
    | STACKED
    | WARNING
    | WHILE
    ;
