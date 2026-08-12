import { useState } from 'react';
import { App, Button, Card, Input, Popconfirm, Space, Table, Tag } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { produtosApi } from '../servicos/produtosApi';
import type { Produto, SalvarProdutoRequest } from '../servicos/produtosApi';
import ModalProduto from '../componentes/ModalProduto';
import { formatarMoeda } from '../../../compartilhado/utils/formatadores';
import { mensagemDoErro } from '../../../infraestrutura/api/cliente';

export default function PaginaProdutos() {
  const { message } = App.useApp();
  const clienteQuery = useQueryClient();

  const [busca, setBusca] = useState('');
  const [modalAberto, setModalAberto] = useState(false);
  const [emEdicao, setEmEdicao] = useState<Produto | null>(null);

  const { data: produtos = [], isFetching } = useQuery({
    queryKey: ['produtos', busca],
    queryFn: () => produtosApi.listar(busca),
  });

  const invalidar = () => clienteQuery.invalidateQueries({ queryKey: ['produtos'] });

  const salvar = useMutation({
    mutationFn: async (dados: SalvarProdutoRequest) => {
      if (emEdicao) {
        await produtosApi.atualizar(emEdicao.id, dados);
        return;
      }

      await produtosApi.criar(dados);
    },
    onSuccess: async () => {
      message.success(emEdicao ? 'Produto atualizado.' : 'Produto cadastrado.');
      setModalAberto(false);
      setEmEdicao(null);
      await invalidar();
    },
    onError: (erro) => message.error(mensagemDoErro(erro, 'Não foi possível salvar o produto.')),
  });

  const excluir = useMutation({
    mutationFn: produtosApi.excluir,
    onSuccess: async () => {
      message.success('Produto excluído.');
      await invalidar();
    },
    onError: (erro) => message.error(mensagemDoErro(erro, 'Não foi possível excluir o produto.')),
  });

  const colunas: ColumnsType<Produto> = [
    { title: 'Código', dataIndex: 'codigo', width: 130 },
    { title: 'Nome', dataIndex: 'nome' },
    { title: 'Unidade', dataIndex: 'unidadeMedida', width: 100 },
    {
      title: 'Custo',
      dataIndex: 'precoCusto',
      width: 130,
      align: 'right',
      render: (valor: number) => formatarMoeda(valor),
    },
    {
      title: 'Venda',
      dataIndex: 'precoVenda',
      width: 130,
      align: 'right',
      render: (valor: number) => formatarMoeda(valor),
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
      render: (_, produto) => (
        <Space>
          <Button
            size="small"
            onClick={() => {
              setEmEdicao(produto);
              setModalAberto(true);
            }}
          >
            Editar
          </Button>
          <Popconfirm
            title="Excluir produto"
            description="Esta ação não pode ser desfeita."
            okText="Excluir"
            cancelText="Cancelar"
            onConfirm={() => excluir.mutate(produto.id)}
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
      title="Produtos"
      extra={
        <Button
          type="primary"
          onClick={() => {
            setEmEdicao(null);
            setModalAberto(true);
          }}
        >
          Novo produto
        </Button>
      }
    >
      <Input.Search
        placeholder="Buscar por código ou nome"
        allowClear
        onSearch={setBusca}
        style={{ maxWidth: 360, marginBottom: 16 }}
      />

      <Table
        rowKey="id"
        columns={colunas}
        dataSource={produtos}
        loading={isFetching}
        size="middle"
        pagination={{ pageSize: 20, showSizeChanger: false }}
        scroll={{ x: 900 }}
      />

      <ModalProduto
        aberto={modalAberto}
        produto={emEdicao}
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
