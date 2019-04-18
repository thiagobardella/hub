# hub

## Tarefa 1

Foi proposto criar um sistema Desktop com interface que permitisse operações de CRUD para Clientes. Este repositório contém o código fonte para a solução implementada em C# a partir do Visual Studio.

Para testar esta solução, favor considerar os seguintes pontos:
* Um cliente é identificado unicamente através de Nome e Email, mas o sistema ainda permite a escolha de uma data de cadastro deste cliente.
* Via interface, o usuário pode cadastrar um novo cliente, atualizar as informações de um cliente já cadastrado, filtrar os clientes já cadastrados por data de registro e texto contido no nome ou email, excluir um cliente e cadastrar múltiplos clientes através de upload de um arquivo excel com as informações cadastrais. 
* Caso algum registro desta planilha não possua algum dado cadastral ou este registro já exista no banco de dados, este registro é ignorado da operação de cadastro.
* A planilha a ser carregada para o sistema deve conter um header (linha 0) com a identificação de cada coluna: Name, Email e Date. O sistema emite mensagem de alerta caso este header esteja incorreto. Há um exemplo de planilha (Multiple Clients.xlsx) válida para melhor compreensão. 
* O tipo de arquivo aceito pela solução é proveniente de uma planilha excel, portanto com terminação (.xlsx). Caso um arquivo qualquer tal como uma imagem, seja carregado para o sistema, este emitirá mensagem de erro para o usuário e nada fará com o arquivo.
* Esta solução possui um instalador próprio que garante melhor experiência para o usuário tester. O instalador, que está na raiz deste repositório (Deploy.msi), carrega o arquivo de script de criação do banco de dados para o mesmo diretório escolhido para instalação e já instala todos os requisitos necessários para o funcionamento do sistema. Ao iniciar a aplicação instalada através do arquivo ClientsRegistration.exe (tipo Aplicativo), que estará no diretório escolhido durante a instalação, o banco já será criado automaticamente.
* Esta solução tem como pré-requisitos a instalação de .NET Framework 4.6.1 e SQL Server, que serão instalados automaticamente após a execução da tarefa acima. Além disso, utiliza as bibliotecas Entity Framework e ExcelDataReader para melhor organização do código e conexão com o banco de dados.

## Tarefa 2

