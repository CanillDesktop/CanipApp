# Como empacotar Frontend + Backend e rodar em qualquer máquina

## Você **já** empacota os dois juntos

Quando você **publica o Frontend**, o projeto automaticamente:

1. Publica o **Backend** (e já pode ser self-contained se você quiser).
2. Copia toda a pasta do Backend para **dentro** da pasta de publicação do Frontend.

Resultado: **uma única pasta** com o executável do Frontend e a subpasta `Backend\`.  
Quem usa só precisa **rodar o executável do Frontend** — o app inicia o Backend sozinho.

---

## Como rodar (para o usuário final)

1. Copie a **pasta inteira** de publicação (não só o .exe).
2. Abra essa pasta e execute o **executável do Frontend** (ex.: `Frontend.exe` ou o nome do app).
3. Não é preciso rodar o Backend manualmente: o Frontend inicia o Backend na pasta `Backend\` ao lado.

Ou seja: **só existe um .exe para abrir** — o do Frontend.

---

## Como publicar (para você, desenvolvedor)

### Opção A — Sem instalar .NET na máquina de destino (recomendado para distribuir)

Tudo self-contained (Frontend e Backend com o runtime .NET embutido). A pasta fica maior, mas roda em qualquer Windows sem instalar nada.

```bash
dotnet publish Frontend\Frontend.csproj -c Release -f net8.0-windows10.0.19041.0 -r win10-x64 --self-contained true
```

- O **Frontend** é publicado self-contained.
- O target do projeto **já publica o Backend** self-contained e copia para `PublishDir\Backend\`.
- Saída típica: `Frontend\bin\Release\net8.0-windows10.0.19041.0\win10-x64\publish\`.

### Opção B — Máquina de destino com .NET 8 instalado

Pacote menor; exige .NET 8 Runtime (Desktop) na máquina.

```bash
dotnet publish Frontend\Frontend.csproj -c Release -f net8.0-windows10.0.19041.0
```

(Não passa `-r` nem `--self-contained true`; o Backend é publicado como framework-dependent.)

---

## Estrutura da pasta após publicar

```
publish\
├── Frontend.exe          ← Único executável que o usuário abre
├── (outros arquivos do Frontend)
└── Backend\
    ├── Backend.exe       ← Iniciado automaticamente pelo Frontend
    ├── appsettings.json
    └── (demais DLLs e dependências)
```

Sempre distribua a **pasta inteira**; não envie só o `Frontend.exe`.

---

## Resumo

| Dúvida | Resposta |
|--------|----------|
| Rodo o Frontend ou o Backend? | Só o **Frontend**. O Backend sobe sozinho. |
| Dá para empacotar os dois juntos? | **Sim.** Ao publicar o Frontend, o Backend já vai junto na pasta `Backend\`. |
| Como não exigir .NET na máquina? | Use `--self-contained true` no comando de publish do Frontend (o Backend já segue essa opção). |
