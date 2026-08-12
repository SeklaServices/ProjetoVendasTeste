import { useState } from 'react';
import { App, Button, Card, Input, Popconfirm, Space, Table, Tag } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { clientesApi, formatarDocumento } from '../servicos/clientesApi';
import type { Cliente, SalvarClienteRequest } from '../servicos/clientesApi';
import ModalCliente from '../componentes/ModalCliente';
import { mensagemDoErro } from '../../../infraestrutura/api/cliente';

export default function PaginaClientes() {
  const { message } = App.useApp();
  const clienteQuery = useQueryClient();

  const [busca, setBusca] = useState('');
  const [modalAberto, setModalAberto] = useState(false);
  const [emEdicao, setEmEdicao] = useState<Cliente | null>(null);

  const { data: clientes = [], isFetching } = useQuery({
    queryKey: ['clientes', busca],
    queryFn: () => clientesApi.listar(busca),
  });

  // Invalida também a lista de ativos, usada pelo seletor da tela de vendas.
  const invalidar = () => clienteQuery.invalidateQueries({ queryKey: ['clientes'] });

  const salvar = useMutation({
    mutationFn: async (dados: SalvarClienteRequest) => {
      if (emEdicao) {
        await clientesApi.atualizar(emEdicao.id, dados);
        return;
      }

      await clientesApi.criar(dados);
    },
    onSuccess: async () => {
      message.success(emEdicao ? 'Cliente atualizado.' : 'Cliente cadastrado.');
      setModalAberto(false);
      setEmEdicao(null);
      await invalidar();
    },
    onError: (erro) => message.error(mensagemDoErro(erro, 'Não foi possível salvar o cliente.')),
  });

  const excluir = useMutation({
    mutationFn: clientesApi.excluir,
    onSuccess: async () => {
      message.success('Cliente excluído.');
      await invalidar();
    },
    onError: (erro) => message.error(mensagemDoErro(erro, 'Não foi possível excluir o cliente.')),
  });

  const colunas: ColumnsType<Cliente> = [
    { title: 'Código', dataIndex: 'codigo', width: 100 },
    { title: 'Nome', dataIndex: 'nome' },
    {
      title: 'CPF / CNPJ',
      dataIndex: 'documento',
      width: 180,
      render: (documento: string | null) => formatarDocumento(documento) || '—',
    },
    {
      title: 'Telefone',
      dataIndex: 'telefone',
      width: 150,
      render: (telefone: string | null) => telefone || '—',
    },
    {
      title: 'E-mail',
      dataIndex: 'email',
      width: 220,
      render: (email: string | null) => email || '—',
    },
    {
      title: 'Situação',
      dataIndex: 'ativo',
      width: 110,
      render: (ativo: boolean) =>
        ativo ? <Tag color="green">Ativo</Tag> : <Tag color="default">Inativo</Tag>,
    },
    {
      title: 'Ações',
      key: 'acoes',
      width: 160,
      render: (_, cliente) => (
        <Space>
          <Button
            size="small"
            onClick={() => {
              setEmEdicao(cliente);
              setModalAberto(true);
            }}
          >
            Editar
          </Button>
          <Popconfirm
            title="Excluir cliente"
            description="Esta ação não pode ser desfeita."
            okText="Excluir"
            cancelText="Cancelar"
            onConfirm={() => excluir.mutate(cliente.id)}
          >
            <Button size="small" danger loading={excluir.isPending}>
              Excluir
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Card
      title="Clientes"
      extra={
        <Button
          type="primary"
          onClick={() => {
            setEmEdicao(null);
            setModalAberto(true);
          }}
        >
          Novo cliente
        </Button>
      }
    >
      <Input.Search
        placeholder="Buscar por código, nome ou documento"
        allowClear
        onSearch={setBusca}
        style={{ maxWidth: 400, marginBottom: 16 }}
      />

      <Table
        rowKey="id"
        columns={colunas}
        dataSource={clientes}
        loading={isFetching}
        size="middle"
        pagination={{ pageSize: 20, showSizeChanger: false }}
        scroll={{ x: 1100 }}
      />

      <ModalCliente
        aberto={modalAberto}
        cliente={emEdicao}
        salvando={salvar.isPending}
        aoCancelar={() => {
          setModalAberto(false);
          setEmEdicao(null);
        }}
        aoSalvar={(dados) => salvar.mutate(dados)}
      />
    </Card>
  );
}
