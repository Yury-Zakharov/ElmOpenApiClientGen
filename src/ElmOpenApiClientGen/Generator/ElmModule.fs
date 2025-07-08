namespace ElmOpenApiClientGen.Generator

type ElmModule =
    {
        /// e.g. "Api.Users"
        QualifiedName: string

        /// File name relative to output dir, e.g. "Api/Users.elm"
        RelativePath: string

        /// Full Elm module source code
        Source: string
    }
