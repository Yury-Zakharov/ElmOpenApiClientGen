namespace ElmOpenApiClientGen.Generator

open System
open System.IO

module Output =

    let private ensureDirExists (path: string) =
        let dir = Path.GetDirectoryName path
        if not (String.IsNullOrWhiteSpace dir) then
            Directory.CreateDirectory dir |> ignore

    let writeModules (outputDir: string) (modules: ElmModule list) (force: bool) : unit =
        for m in modules do
            let fullPath = Path.Combine(outputDir, m.RelativePath)
            let fileExists = File.Exists(fullPath)

            if fileExists && not force then
                printfn $"[SKIP] {fullPath} exists (use --force to overwrite)"
            else
                ensureDirExists fullPath
                File.WriteAllText(fullPath, m.Source)
                printfn $"[WRITE] {fullPath}"
