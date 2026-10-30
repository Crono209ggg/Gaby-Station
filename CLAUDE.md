# Fluxo de trabalho para features

Sempre que o usuário pedir uma feature, mudança de código ou correção neste
projeto (Space Station 14 / fork Gaby-Station), siga esta ordem sem precisar
que o usuário peça cada etapa:

## 1. Grill (perguntar antes de codar)

Antes de escrever qualquer código, faça as perguntas necessárias para não
assumir nada errado. No SS14, confirme em especial:

- Isso roda no servidor, no cliente ou em `Shared`?
- Precisa de prediction?
- É um componente/sistema novo ou dá para reaproveitar um existente?
- Mexe em arquivo do upstream (gera conflito futuro) ou fica no namespace do fork?
- Onde ficam os prototypes/YAML afetados?

Para mudança pequena e óbvia (ajuste de YAML, balanceamento, tweak de um
valor), pule direto para a implementação — não faça perguntas desnecessárias.
Só faça o "grill" quando houver decisão real em aberto.

## 2. Implement (implementar)

Faça a mudança seguindo os padrões do projeto:

- Respeite o namespace do fork; não edite arquivos do upstream sem necessidade.
- Siga os padrões de ECS do SS14 (componentes só com dados, lógica no sistema,
  `[Dependency]` e `SubscribeLocalEvent` corretos).
- Mantenha a mudança mínima: só o que a tarefa pede.

## 3. Code Review (revisar)

Ao terminar, rode a skill `/code-review` sobre o diff antes de considerar a
tarefa concluída, e corrija o que ela apontar que fizer sentido para o SS14.
