import { useState } from 'react';
import { App, Button, Card, DatePicker, Popconfirm, Space, Table } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { Dayjs } from 'dayjs';
import { comprasApi } from '../../movimentos/servicos/movimentosApi';
import type { CompraResumo } from '../../movimentos/servicos/movimentosApi';
import ModalDocumento from '../../../compartilhado/componentes/ModalDocumento';
import { formatarData, formatarMoeda } from '../../../compartilhado/utils/formatadores';
import { mensagemDoErro } from '../../../infraestrutura/api/cliente';

export default function PaginaCompras() {
  const { message } = App.useApp();
  const clienteQuery = useQueryClient();

  const [periodo, setPeriodo] = useState<[Dayjs, Dayjs] | null>(null);
  const [modalAberto, setModalAberto] = useState(false);

  const filtro = periodo
    ? { dataInicial: periodo[0].format('YYYY-MM-DD'), dataFinal: periodo[1].format('YYYY-MM-DD') }
    : {};

  const { data: compras = [], isFetching } = useQuery({
    queryKey: ['compras', filtro],
    queryFn: () => comprasApi.listar(filtro),
  });

  const invalidar = async () => {
    await clienteQuery.invalidateQueries({ queryKey: ['compras'] });
    await clienteQuery.invalidateQueries({ queryKey: ['resumo'] });
  };

  const criar = useMutation({
    mutationFn: (dados: Parameters<typeof comprasApi.criar>[0]) => comprasApi.criar(dados),
    onSuccess: async (resultado) => {
      message.success(`Compra nº ${resultado.numero} registrada.`);
      setModalAberto(false);
      await invalidar();
    },
    onError: (erro) => message.error(mensagemDoErro(erro, 'Não foi possível registrar a compra.')),
  });

  const excluir = useMutation({
    mutationFn: comprasApi.excluir,
    onSuccess: async () => {
      message.success('Compra excluída.');
      await invalidar();
    },
    onError: (erro) => message.error(mensagemDoErro(erro, 'Não foi possível excluir a compra.')),
  });

  const colunas: ColumnsType<CompraResumo> = [
    { title: 'Nº', dataIndex: 'numero', width: 90 },
    { title: 'Data', dataIndex: 'data', width: 130, render: (iso: string) => formatarData(iso) },
    { title: 'Fornecedor', dataIndex: 'fornecedor' },
    { title: 'Itens', dataIndex: 'quantidadeItens', width: 90, align: 'right' },
    {
      title: 'Valor total',
      dataIndex: 'valorTotal',
      width: 150,
      align: 'right',
      render: (valor: number) => formatarMoeda(valor),
    },
    {
      title: 'Ações',
      key: 'acoes',
      width: 110,
      render: (_, compra) => (
        <Popconfirm
          title="Excluir compra"
          description="Esta ação não pode ser desfeita."
          okText="Excluir"
          cancelText="Cancelar"
          onConfirm={() => excluir.mutate(compra.id)}
        >
          <Button size="small" danger loading={excluir.isPending}>
            Excluir
          </Button>
        </Popconfirm>
      ),
    },
  ];

  return (
    <Card
      title="Compras"
      extra={
        <Button type="primary" onClick={() => setModalAberto(true)}>
          Nova compra
        </Button>
      }
    >
      <Space style={{ marginBottom: 16 }}>
        <DatePicker.RangePicker
          format="DD/MM/YYYY"
          onChange={(valores) =>
            setPeriodo(valores && valores[0] && valores[1] ? [valores[0], valores[1]] : null)
          }
        />
      </Space>

      <Table
        rowKey="id"
        columns={colunas}
        dataSource={compras}
        loading={isFetching}
        size="middle"
        pagination={{ pageSize: 20, showSizeChanger: false }}
        scroll={{ x: 800 }}
      />

      <ModalDocumento
        aberto={modalAberto}
        salvando={criar.isPending}
        titulo="Nova compra"
        rotuloParceiro="Fornecedor"
        tipoParceiro="texto"
        precoSugerido="precoCusto"
        aoCancelar={() => setModalAberto(false)}
        aoSalvar={({ data, parceiro, observacao, itens }) =>
          criar.mutate({ data, fornecedor: parceiro, observacao, itens })
        }
      />
    </Card>
  );
}
