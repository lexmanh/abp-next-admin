<template>
  <Card
    :tab-list="tabListTitle"
    v-bind="$attrs"
    :active-tab-key="activeKey"
    @tab-change="onTabChange"
  >
    <p v-if="activeKey === 'tab1'">
      <VisitAnalysis />
    </p>
    <p v-if="activeKey === 'tab2'">
      <VisitAnalysisBar />
    </p>
  </Card>
</template>
<script lang="ts" setup>
  import { ref } from 'vue';
  import { Card } from 'ant-design-vue';
  import VisitAnalysis from './VisitAnalysis.vue';
  import VisitAnalysisBar from './VisitAnalysisBar.vue';

  const activeKey = ref('tab1');

  const tabListTitle = [
    {
      key: 'tab1',
      tab: 'Xu hướng',
    },
    {
      key: 'tab2',
      tab: 'Số lần truy cập',
    },
  ];

  function onTabChange(key) {
    activeKey.value = key;
  }
</script>
