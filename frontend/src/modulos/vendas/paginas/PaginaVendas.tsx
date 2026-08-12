import { useState } from 'react';
import { App, Button, Card, DatePicker, Popconfirm, Space, Table } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { Dayjs } from 'dayjs';
import { vendasApi } from '../../movimentos/servicos/movimentosApi';
import type { VendaResumo } from '../../movimentos/servicos/movimentosApi';
import ModalDocumento from '../../../compartilhado/componentes/ModalDocumento';
import { formatarData, formatarMoeda } from '../../../compartilhado/utils/formatadores';
import { mensagemDoErro } from '../../../infraestrutura/api/cliente';

export default function PaginaVendas() {
  const { message } = App.useApp();
  const clienteQuery = useQueryClient();

  const [periodo, setPeriodo] = useState<[Dayjs, Dayjs] | null>(null);
  const [modalAberto, setModalAberto] = useState(false);

  const filtro = periodo
    ? { dataInicial: periodo[0].format('YYYY-MM-DD'), dataFinal: periodo[1].format('YYYY-MM-DD') }
    : {};

  const { data: vendas = [], isFetching } = useQuery({
    queryKey: ['vendas', filtro],
    queryFn: () => vendasApi.listar(filtro),
  });

  const invalidar = async () => {
    await clienteQuery.invalidateQueries({ queryKey: ['vendas'] });
    await clienteQuery.invalidateQueries({ queryKey: ['resumo'] });
  };

  const criar = useMutation({
    mutationFn: (dados: Parameters<typeof vendasApi.criar>[0]) => vendasApi.criar(dados),
    onSuccess: async (resultado) => {
      message.success(`Venda nº ${resultado.numero} registrada.`);
      setModalAberto(false);
      await invalidar();
    },
    onError: (erro) => message.error(mensagemDoErro(erro, 'Não foi possível registrar a venda.')),
  });

  const excluir = useMutation({
    mutationFn: vendasApi.excluir,
    onSuccess: async () => {
      message.success('Venda excluída.');
      await invalidar();
    },
    onError: (erro) => message.error(mensagemDoErro(erro, 'Não foi possível excluir a venda.')),
  });

  const colunas: ColumnsType<VendaResumo> = [
    { title: 'Nº', dataIndex: 'numero', width: 90 },
    { title: 'Data', dataIndex: 'data', width: 130, render: (iso: string) => formatarData(iso) },
    { title: 'Cliente', dataIndex: 'cliente' },
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
      render: (_, venda) => (
        <Popconfirm
          title="Excluir venda"
          description="Esta ação não pode ser desfeita."
          okText="Excluir"
          cancelText="Cancelar"
          onConfirm={() => excluir.mutate(venda.id)}
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
      title="Vendas"
      extra={
        <Button type="primary" onClick={() => setModalAberto(true)}>
          Nova venda
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
        dataSource={vendas}
        loading={isFetching}
        size="middle"
        pagination={{ pageSize: 20, showSizeChanger: false }}
        scroll={{ x: 800 }}
      />

      <ModalDocumento
        aberto={modalAberto}
        salvando={criar.isPending}
        titulo="Nova venda"
        rotuloParceiro="Cliente"
        precoSugerido="precoVenda"
        aoCancelar={() => setModalAberto(false)}
        aoSalvar={({ data, parceiro, observacao, itens }) =>
          criar.mutate({ data, cliente: parceiro, observacao, itens })
        }
      />
    </Card>
  );
}
