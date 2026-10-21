-- Se ejecuta sola la primera vez que se crea el volumen de postgres
CREATE EXTENSION IF NOT EXISTS vector; 		-- embeddings para la IA (pgvector)
CREATE EXTENSION IF NOT EXISTS pgcrypto; 	-- gen_random_uuid()
CREATE EXTENSION IF NOT EXISTS citext;		-- correos sin distinguir mayusculas
