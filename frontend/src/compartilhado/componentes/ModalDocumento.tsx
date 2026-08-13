import { useEffect, useState } from 'react';
import { App, Button, Col, DatePicker, Form, Input, InputNumber, Modal, Row, Select, Space, Table, Typography } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useQuery } from '@tanstack/react-query';
import dayjs from 'dayjs';
import type { Dayjs } from 'dayjs';
import { produtosApi } from '../../modulos/produtos/servicos/produtosApi';
import type { Produto } from '../../modulos/produtos/servicos/produtosApi';
import { clientesApi } from '../../modulos/clientes/servicos/clientesApi';
import type { Cliente } from '../../modulos/clientes/servicos/clientesApi';
import type { ItemRequest } from '../../modulos/movimentos/servicos/movimentosApi';
import { formatarMoeda, formatarQuantidade } from '../utils/formatadores';

/**
 * Formulário de lançamento de compra ou venda.
 *
 * Compra e venda são a mesma tela com rótulos e sugestão de preço diferentes — por isso um
 * componente só, parametrizado. No BACKEND as duas estão duplicadas de propósito; aqui compartilhar
 * é a escolha certa porque não há regra de negócio envolvida, só apresentação.
 *
 * O parceiro é o ponto onde as duas divergem hoje: a venda escolhe um CLIENTE CADASTRADO, e a
 * compra ainda digita o fornecedor como texto livre. Daí o `tipoParceiro`.
 */
interface Props {
  aberto: boolean;
  salvando: boolean;
  titulo: string;
  rotuloParceiro: string;
  /** `texto` = campo livre (fornecedor). `cliente` = seleção do cadastro de clientes. */
  tipoParceiro: 'texto' | 'cliente';
  /** De onde sai o preço sugerido ao escolher o produto. */
  precoSugerido: 'precoCusto' | 'precoVenda';
  aoCancelar: () => void;
  aoSalvar: (dados: {
    data: string;
    /** Nome digitado quando `tipoParceiro` é `texto`; id do cliente quando é `cliente`. */
    parceiro: string;
    observacao: string | null;
    itens: ItemRequest[];
  }) => void;
}

interface Cabecalho {
  data: Dayjs;
  parceiro: string;
  observacao?: string;
}

interface ItemNaTela extends ItemRequest {
  chave: string;
  produtoNome: string;
  unidadeMedida: string;
}

export default function ModalDocumento({
  aberto,
  salvando,
  titulo,
  rotuloParceiro,
  tipoParceiro,
  precoSugerido,
  aoCancelar,
  aoSalvar,
}: Props) {
  const { message } = App.useApp();
  const [form] = Form.useForm<Cabecalho>();
  const [itens, setItens] = useState<ItemNaTela[]>([]);

  const [produtoId, setProdutoId] = useState<string | undefined>();
  const [quantidade, setQuantidade] = useState<number | null>(1);
  const [preco, setPreco] = useState<number | null>(0);

  // Só produtos ativos: inativo é rejeitado pelo backend, então nem oferecemos.
  const { data: produtos = [] } = useQuery({
    queryKey: ['produtos', 'ativos'],
    queryFn: () => produtosApi.listar(undefined, true),
    enabled: aberto,
  });

  // Mesma lógica para clientes — e só busca quando a tela realmente usa o cadastro.
  const { data: clientes = [] } = useQuery({
    queryKey: ['clientes', 'ativos'],
    queryFn: () => clientesApi.listar(undefined, true),
    enabled: aberto && tipoParceiro === 'cliente',
  });

  useEffect(() => {
    if (aberto) {
      form.resetFields();
      form.setFieldsValue({ data: dayjs() });
      setItens([]);
      limparLinhaDeItem();
    }
  }, [aberto, form]);

  function limparLinhaDeItem() {
    setProdutoId(undefined);
    setQuantidade(1);
    setPreco(0);
  }

  function aoEscolherProduto(id: string) {
    setProdutoId(id);
    const produto = produtos.find((p: Produto) => p.id === id);
    setPreco(produto ? produto[precoSugerido] : 0);
  }

  function adicionarItem() {
    if (!produtoId) {
      message.warning('Escolha o produto.');
      return;
    }

    if (!quantidade || quantidade <= 0) {
      message.warning('A quantidade deve ser maior que zero.');
      return;
    }

    const produto = produtos.find((p: Produto) => p.id === produtoId);
    if (!produto) {
      return;
    }

    setItens((atuais) => [
      ...atuais,
      {
        // Date.now() não serve como chave: dois cliques rápidos colidem. Um contador do tamanho
        // atual também não, porque remover no meio repete a chave.
        chave: crypto.randomUUID(),
        produtoId,
        quantidade,
        precoUnitario: preco ?? 0,
        produtoNome: `${produto.codigo} — ${produto.nome}`,
        unidadeMedida: produto.unidadeMedida,
      },
    ]);

    limparLinhaDeItem();
  }

  const total = itens.reduce((soma, item) => soma + item.quantidade * item.precoUnitario, 0);

  const colunas: ColumnsType<ItemNaTela> = [
    { title: 'Produto', dataIndex: 'produtoNome' },
    { title: 'Un.', dataIndex: 'unidadeMedida', width: 70 },
    {
      title: 'Quantidade',
      dataIndex: 'quantidade',
      width: 120,
      align: 'right',
      render: (valor: number) => formatarQuantidade(valor),
    },
    {
      title: 'Preço',
      dataIndex: 'precoUnitario',
      width: 120,
      align: 'right',
      render: (valor: number) => formatarMoeda(valor),
    },
    {
      title: 'Subtotal',
      key: 'subtotal',
      width: 130,
      align: 'right',
      render: (_, item) => formatarMoeda(item.quantidade * item.precoUnitario),
    },
    {
      title: '',
      key: 'acoes',
      width: 90,
      render: (_, item) => (
        <Button
          size="small"
          danger
          onClick={() => setItens((atuais) => atuais.filter((i) => i.chave !== item.chave))}
        >
          Remover
        </Button>
      ),
    },
  ];

  function enviar(cabecalho: Cabecalho) {
    if (itens.length === 0) {
      message.warning('Adicione pelo menos um item.');
      return;
    }

    aoSalvar({
      data: cabecalho.data.format('YYYY-MM-DD'),
      parceiro: cabecalho.parceiro,
      observacao: cabecalho.observacao?.trim() || null,
      itens: itens.map(({ produtoId: id, quantidade: qtd, precoUnitario }) => ({
        produtoId: id,
        quantidade: qtd,
        precoUnitario,
      })),
    });
  }

  return (
    <Modal
      open={aberto}
      title={titulo}
      width={900}
      okText="Salvar"
      cancelText="Cancelar"
      confirmLoading={salvando}
      onCancel={aoCancelar}
      onOk={() => form.submit()}
      destroyOnHidden
    >
      <Form form={form} layout="vertical" onFinish={enviar} disabled={salvando}>
        <Row gutter={16}>
          <Col span={6}>
            <Form.Item name="data" label="Data" rules={[{ required: true, message: 'Informe a data.' }]}>
              <DatePicker
                format="DD/MM/YYYY"
                style={{ width: '100%' }}
                // Data futura é rejeitada pelo backend; bloquear aqui evita o erro desnecessário.
                disabledDate={(data) => data.isAfter(dayjs(), 'day')}
              />
            </Form.Item>
          </Col>
          <Col span={10}>
            <Form.Item
              name="parceiro"
              label={rotuloParceiro}
              rules={[
                { required: true, message: `Informe o ${rotuloParceiro.toLowerCase()}.` },
                ...(tipoParceiro === 'texto' ? [{ max: 120 }] : []),
              ]}
            >
              {tipoParceiro === 'cliente' ? (
                <Select
                  showSearch
                  placeholder="Selecione o cliente"
                  optionFilterProp="label"
                  // Cliente inativo é recusado pelo backend, então nem entra na lista.
                  options={clientes.map((c: Cliente) => ({
                    value: c.id,
                    label: `${c.codigo} — ${c.nome}`,
                  }))}
                  notFoundContent="Nenhum cliente ativo cadastrado."
                />
              ) : (
                <Input />
              )}
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item name="observacao" label="Observação" rules={[{ max: 500 }]}>
              <Input />
            </Form.Item>
          </Col>
        </Row>
      </Form>

      <Typography.Title level={5}>Itens</Typography.Title>

      <Space.Compact style={{ width: '100%', marginBottom: 12 }}>
        <Select
          showSearch
          placeholder="Produto"
          style={{ width: '45%' }}
          value={produtoId}
          onChange={aoEscolherProduto}
          disabled={salvando}
          optionFilterProp="label"
          options={produtos.map((p: Produto) => ({
            value: p.id,
            label: `${p.codigo} — ${p.nome}`,
          }))}
        />
        <InputNumber
          placeholder="Quantidade"
          style={{ width: '20%' }}
          min={0}
          precision={3}
          decimalSeparator=","
          value={quantidade}
          onChange={setQuantidade}
          disabled={salvando}
        />
        <InputNumber
          placeholder="Preço"
          style={{ width: '20%' }}
          min={0}
          precision={2}
          decimalSeparator=","
          value={preco}
          onChange={setPreco}
          disabled={salvando}
        />
        <Button type="primary" style={{ width: '15%' }} onClick={adicionarItem} disabled={salvando}>
          Adicionar
        </Button>
      </Space.Compact>

      <Table
        rowKey="chave"
        size="small"
        columns={colunas}
        dataSource={itens}
        pagination={false}
        locale={{ emptyText: 'Nenhum item adicionado.' }}
        summary={() => (
          <Table.Summary.Row>
            <Table.Summary.Cell index={0} colSpan={4}>
              <strong>Total</strong>
            </Table.Summary.Cell>
            <Table.Summary.Cell index={1} align="right">
              <strong>{formatarMoeda(total)}</strong>
            </Table.Summary.Cell>
            <Table.Summary.Cell index={2} />
          </Table.Summary.Row>
        )}
      />
    </Modal>
  );
}
